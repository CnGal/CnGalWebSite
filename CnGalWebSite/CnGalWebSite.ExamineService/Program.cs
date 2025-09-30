using CnGalWebSite.Core.Services;
using CnGalWebSite.EamineService;
using CnGalWebSite.EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetCore.AutoRegisterDi;
using NLog;
using NLog.Extensions.Logging;
using System.Reflection;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddNLog();
});

// 添加后台任务
builder.Services.AddHostedService<Worker>();

// 添加事件总线
builder.Services.AddEventBus();

// 添加用户机密
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

//自动依赖注入
builder.Services.RegisterAssemblyPublicNonGenericClasses()
  .Where(c => (c.Name.EndsWith("Service") || c.Name.EndsWith("Provider")) && c.Name.StartsWith("Background") == false)
  .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);

using IHost host = builder.Build();

host.Run();
