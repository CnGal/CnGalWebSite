using CnGalWebSite.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpOverrides;
using CnGalWebSite.RobotClientX.Services;
using NetCore.AutoRegisterDi;
using CnGalWebSite.RobotClientX.DataRepositories;
using CnGalWebSite.Core.Services.Query;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//添加HTTP请求
builder.Services.AddSingleton(sp => new HttpClient());
//自定义服务
builder.Services.AddScoped<IHttpService, HttpService>();
//设置主题
builder.Services.AddMasaBlazor(s => s.ConfigureTheme(s =>
{
    s.Themes.Light.Primary =builder.Configuration["WebSiteTheme"];
}));
//添加状态检查
builder.Services.AddHealthChecks();
//添加真实IP
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
//添加后台定时任务
builder.Services.AddHostedService<BackgroundTaskService>();
//添加仓储
builder.Services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
//自动依赖注入
builder.Services.RegisterAssemblyPublicNonGenericClasses()
  .Where(c => (c.Name.EndsWith("Service") || c.Name.EndsWith("Provider")) && c.Name.StartsWith("Background")==false)
  .AsPublicImplementedInterfaces(ServiceLifetime.Singleton);
//添加Query
builder.Services.AddScoped<IQueryService, QueryService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
//转发IP
_ = app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

//添加状态检查终结点
app.UseHealthChecks("/healthz");

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
