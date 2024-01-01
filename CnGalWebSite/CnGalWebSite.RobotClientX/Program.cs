using CnGalWebSite.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpOverrides;
using CnGalWebSite.RobotClientX.Services;
using NetCore.AutoRegisterDi;
using CnGalWebSite.RobotClientX.DataRepositories;
using CnGalWebSite.Core.Services.Query;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using BlazorComponent;
using Masa.Blazor.Presets;
using Masa.Blazor;
using CnGalWebSite.EventBus.Extensions;
using CnGalWebSite.EventBus.Services;

var builder = WebApplication.CreateBuilder(args);
//自动重置配置
builder.Configuration.AddJsonFile("appsettings.json", true, reloadOnChange: true);
builder.Configuration.AddJsonFile(Path.Combine(builder.Environment.WebRootPath,"Data","Setting.json"), true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//添加HTTP请求
builder.Services.AddSingleton(sp => new HttpClient());
//自定义服务
builder.Services.AddScoped<IHttpService, HttpService>();
//修改Masa主题
builder.Services.AddMasaBlazor(options =>
{
    //主题
    options.ConfigureTheme(s =>
    {
        if (DateTime.Now.Day == 1 && DateTime.Now.Month == 4)
        {
            s.Themes.Light.Primary = "#4CAF50";
        }
        else
        {
            s.Themes.Light.Primary = "#f06292";
        }
        s.Themes.Dark.Primary = "#0078BF";
    });
});//添加状态检查
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
//API终结点
builder.Services.AddEndpointsApiExplorer();
//注册Swagger生成器，定义一个或多个Swagger文件
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CnGal资料站 - 看板娘 API",
        Description = "这里是看板娘哦~",
        Contact = new OpenApiContact
        {
            Name = "CnGal",
            Email = "help@cngal.org"
        },
        Version = "v1"
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
//添加控制器
builder.Services.AddControllers();
//事件总线
builder.Services.AddEventBus();

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
//启用中间件Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CnGal API V1");
});
//添加状态检查终结点
app.UseHealthChecks("/healthz");

app.UseRouting();
app.MapDefaultControllerRoute();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
