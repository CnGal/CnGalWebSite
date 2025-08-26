using CnGalWebSite.EventBus.Extensions;
using Microsoft.Extensions.Configuration;
using NetCore.AutoRegisterDi;
using System.Reflection;
using CnGalWebSite.Kanban.ChatGPT.Extensions;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// 添加后台任务
builder.Services.AddHostedService<Worker>();

// 添加事件总线
builder.Services.AddEventBus();

// 添加用户机密
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

// 添加内存缓存
builder.Services.AddMemoryCache();

// 添加用户个性化服务
builder.Services.AddUserProfileServices();

//自动依赖注入
builder.Services.RegisterAssemblyPublicNonGenericClasses()
  .Where(c => (c.Name.EndsWith("Service") || c.Name.EndsWith("Provider")) && c.Name.StartsWith("Background") == false)
  .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);

//添加HTTP请求
builder.Services.AddSingleton(sp => new HttpClient());

using IHost host = builder.Build();

host.Run();
