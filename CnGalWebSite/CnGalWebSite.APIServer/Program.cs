using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NLog;
using NLog.Web;
using System;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace CnGalWebSite.APIServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Early init of NLog to allow startup and exception logging, before host is built
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddCommandLine(args);//设置添加命令行
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddUserSecrets<Startup>();
                })
                 .ConfigureLogging(logging =>
                 {
                     logging.ClearProviders();
                     logging.SetMinimumLevel(LogLevel.Trace);
                 })
                 .UseNLog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseDefaultServiceProvider(s =>
                    {
                        s.ValidateScopes = true;
                    });
                });
        }
    }
}
