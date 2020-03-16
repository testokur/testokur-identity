namespace TestOkur.Identity
{
    using HealthChecks.UI.Client;
    using IdentityServer4.Configuration;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Prometheus;
    using SpanJson.AspNetCore.Formatter;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using TestOkur.Identity.Configuration;
    using TestOkur.Identity.Infrastructure;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Identity.Services;
    using TestOkur.Infrastructure.Mvc.Extensions;
    using TestOkur.Serialization;

    public class Startup
    {
        private const string CorsPolicyName = "EnableAll";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        private AppConfiguration AppConfiguration { get; } = new AppConfiguration();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            AddOptions(services);
            services.AddResponseCompression();
            services.AddCors(o => o.AddPolicy(CorsPolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            services.AddControllersWithViews()
                .AddSpanJsonCustom<ApiResolver<byte>>();
            services.AddDataProtection();
            AddHealthChecks(services);
            AddDbContext(services);
            AddAuthorization(services);
            AddIdentityServer(services);
            AddAuthentication(services);
            services.AddTransient<IEventSink, EventSink>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(CorsPolicyName);
            app.UseHttpMetrics();
            app.UseMetricServer("/metrics-core");
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            });
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });
                endpoints.MapControllerRoute(
                    "default", "{controller=Account}/{action=Login}/{id?}");
            });
        }

        protected virtual TokenValidationParameters CreateTokenValidationParameters()
        {
            var tokenValidationParameters = new TokenValidationParameters();
            Configuration.GetSection("TokenValidationParameters").Bind(tokenValidationParameters);

            return tokenValidationParameters;
        }

        private void AddAuthentication(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var tokenValidationParameters = CreateTokenValidationParameters();
            tokenValidationParameters.IssuerSigningKey = new X509SecurityKey(new X509Certificate2(Path.Combine("cert", "testokur.pfx"), AppConfiguration.CertificatePassword));

            services
                .AddAuthentication()
                .AddJwtBearer(options => { options.TokenValidationParameters = tokenValidationParameters; });
        }

        private void AddAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    AuthorizationPolicies.Private,
                    policy => policy.RequireAssertion(context =>
                        context.User.IsInRole(Roles.Admin) ||
                        context.User.HasClaim(c => c.Type == JwtClaimTypes.ClientId &&
                                                   c.Value == Clients.Private)));

                options.AddPolicy(AuthorizationPolicies.Customer, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole(Roles.Admin) ||
                        context.User.IsInRole(Roles.Customer)));

                options.AddPolicy(AuthorizationPolicies.Admin, policy =>
                    policy.RequireRole(Roles.Admin));
            });
        }

        private void AddHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("Postgres"));
        }

        private void AddDbContext(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("Postgres");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(
                    connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly)).Options;
            services.AddSingleton(dbContextOptions);
            services.AddSingleton<ApplicationDbContextFactory>();
            services.AddDbContext<ApplicationDbContext>();
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 500;
                options.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        }

        private void AddIdentityServer(IServiceCollection services)
        {
            var builder = services.AddIdentityServer(options =>
            {
                options.Events = new EventsOptions()
                {
                    RaiseErrorEvents = true,
                    RaiseFailureEvents = true,
                    RaiseInformationEvents = true,
                    RaiseSuccessEvents = true,
                };
                options.Authentication.CookieLifetime = TimeSpan.FromDays(30);
                options.Authentication.CookieSlidingExpiration = true;
            });
            builder.AddInMemoryApiResources(Configuration.GetSection("ApiResources"));
            builder.AddInMemoryClients(Configuration.GetSection("Clients"));
            builder.AddSigningCredential(new X509Certificate2(Path.Combine("cert", "testokur.pfx"), AppConfiguration.CertificatePassword));
            builder.AddInMemoryIdentityResources(GetIdentityResources());
            builder.AddAspNetIdentity<ApplicationUser>();
            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            builder.AddProfileService<ProfileService>();
        }

        private IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        private void AddOptions(IServiceCollection services)
        {
            services.AddOptions();
            Configuration.GetSection("AppConfiguration").Bind(AppConfiguration);
            services.ConfigureAndValidate<AppConfiguration>(Configuration);

            services.AddSingleton(resolver =>
                resolver.GetRequiredService<IOptions<AppConfiguration>>().Value);
        }
    }
}
