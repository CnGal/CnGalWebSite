using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.Helper.Helper;
using CnGalWebSite.RobotClient;
using CnGalWebSite.RobotClient.DataRepositories;
using CnGalWebSite.RobotClient.Extentions;
using CnGalWebSite.RobotClient.Services.Events;
using CnGalWebSite.RobotClient.Services.ExternalDatas;
using CnGalWebSite.RobotClient.Services.Messages;
using CnGalWebSite.RobotClient.Services.QQClients;
using CnGalWebSite.RobotClient.Services.SensitiveWords;
using CnGalWebSite.RobotClient.Services.Synchronous;
using Masuda.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore.AutoRegisterDi;
using System.Linq.Dynamic.Core.Tokenizer;

MasudaBot asudaBot
    = new(1, "", "");

//设置Json格式化配置
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());

//添加依赖注入
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddCommandLine(args);//设置添加命令行
    })
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("BackUp/Secrets.json",
            optional: true,
            reloadOnChange: true);
    })
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
        services.AddSingleton(sp => new HttpClient());

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
