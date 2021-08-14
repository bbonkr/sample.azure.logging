using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

namespace Sample.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var azureAppService = Convert.ToBoolean(Environment.GetEnvironmentVariable("ASPNETCORE_AZURE_APP_SERVICE"));

            var loggerConfiguration = new LoggerConfiguration()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .WriteTo.Debug() // [optional] For Visual Studio Ouput Window
               ;

            if (azureAppService)
            {
                // IF ASPNETCORE_AZURE_APP_SERVICE is true, Write log file
                loggerConfiguration = loggerConfiguration.WriteTo.File(
                    @"D:\home\LogFiles\Application\myapp.txt",
                    fileSizeLimitBytes: 1_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1));
            }

            // This logger does not appect service provider.
            Log.Logger = loggerConfiguration
                .CreateLogger();

            try
            {
                Log.Information("Starting host");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) =>
                {
                    var azureAppService = Convert.ToBoolean(Environment.GetEnvironmentVariable("ASPNETCORE_AZURE_APP_SERVICE"));

                    // You must configure logger again for service provider
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.Debug() // [optional] For Visual Studio Ouput Window
                        ;

                    if (azureAppService)
                    {
                        // IF ASPNETCORE_AZURE_APP_SERVICE is true, Write log file
                        configuration.WriteTo.File(
                            @"D:\home\LogFiles\Application\myapp.txt",
                            fileSizeLimitBytes: 1_000_000,
                            rollOnFileSizeLimit: true,
                            shared: true,
                            flushToDiskInterval: TimeSpan.FromSeconds(1));
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
