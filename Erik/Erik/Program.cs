using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Erik;

namespace reasulus.api
{
    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Miscellaneous Design", "AV1210:Catch a specific exception instead of Exception, SystemException or ApplicationException", Justification = "Catch exception to properly log host termination")]
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = BuildConfiguration(env);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config).WriteTo.Console()
                .CreateLogger();
            Log.Logger.Information("Starting {app} {version} - {env}",
                Assembly.GetExecutingAssembly().GetName().Name,
                Assembly.GetExecutingAssembly().GetName().Version,
                env);

            try
            {
                var builder = CreateHostBuilder(args, config);
                var host = builder.Build();
                host.Run();

            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }


        private static IConfigurationRoot BuildConfiguration(string? env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot conf)
        {
            return Host.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSerilog()
                .ConfigureLogging((context, builder) => {
                    builder.ClearProviders();

                    if (context.HostingEnvironment.IsDevelopment() || context.HostingEnvironment.IsStaging())
                    {
                        builder.AddDebug();
                    }
                })
                .ConfigureAppConfiguration((hostBuilderContext, config) => {
                    config.AddConfiguration(conf);
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
