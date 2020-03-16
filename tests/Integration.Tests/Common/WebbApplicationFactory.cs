namespace Integration.Tests.Common
{
    using Integration.Tests.Helper;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using TestOkur.Identity;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Infrastructure.Mvc.Extensions;

    public class WebbApplicationFactory : WebApplicationFactory<Startup>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            AsyncHelper.RunSync(() => host.MigrateDbContextAsync<ApplicationDbContext>(async (context, services) =>
            {
                await Seeder.SeedAsync(services);
            }));

            return host;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseEnvironment("Development")
                        .UseStartup<Startup>();
                });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseEnvironment("Development")
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext())
                .UseStartup<Startup>();
        }
    }
}