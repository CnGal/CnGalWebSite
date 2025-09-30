using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.RobotClient;
using CnGalWebSite.RobotClient.DataRepositories;
using CnGalWebSite.RobotClient.Extentions;
using CnGalWebSite.RobotClient.Services.Events;
using CnGalWebSite.RobotClient.Services.ExternalDatas;
using CnGalWebSite.RobotClient.Services.Https;
using CnGalWebSite.RobotClient.Services.Messages;
using CnGalWebSite.RobotClient.Services.QQClients;
using CnGalWebSite.RobotClient.Services.SensitiveWords;
using CnGalWebSite.RobotClient.Services.Synchronous;
using Masuda.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.AutoRegisterDi;
using NLog;
using NLog.Web;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    MasudaBot asudaBot
        = new(1, "", "");

    //添加依赖注入
    using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddCommandLine(args);//设置添加命令行
    })
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddUserSecrets<Program>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    })
    .UseNLog()
    .ConfigureServices((builder, services) =>
    {
        //添加仓储
        services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
        //添加HTTP请求
        services.AddSingleton(sp => new HttpClient
        {
            BaseAddress = new Uri(builder.Configuration["WebApiPath"])
        });
        services.AddSingleton<IHttpService, HttpService>();
        //自动依赖注入
        services.RegisterAssemblyPublicNonGenericClasses()
          .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Provider"))
          .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);
    })
    .Build();




//获取服务提供程序
using IServiceScope serviceScope = host.Services.CreateScope();
IServiceProvider provider = serviceScope.ServiceProvider;

var _synchronousService = provider.GetRequiredService<ISynchronousService>();
await _synchronousService.RefreshAsync();

var _QQClientService = provider.GetRequiredService<IQQClientService>();
await _QQClientService.Init();


await host.RunAsync();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
