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
