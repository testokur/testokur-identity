namespace TestOkur.Identity
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Prometheus.DotNetRuntime;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using TestOkur.Identity.Infrastructure.Data;
    using TestOkur.Infrastructure.Mvc.Extensions;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetRuntimeStatsBuilder.Default().WithErrorHandler(e =>
            {
                Console.WriteLine(e.ToString());
            }).StartCollecting();

            var host = CreateHostBuilder(args).Build();
            await host.MigrateDbContextAsync<ApplicationDbContext>(async (context, services) =>
            {
                await Seeder.SeedAsync(services);
            });

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .MinimumLevel.Warning()
                            .Enrich.FromLogContext()
                            .WriteTo.Seq(hostingContext.Configuration.GetValue<string>("AppConfiguration:SeqUrl"))
                            .WriteTo.Console())
                        .UseStartup<Startup>();
                });
    }
}
