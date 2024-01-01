using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace CnGalWebSite.APIServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
