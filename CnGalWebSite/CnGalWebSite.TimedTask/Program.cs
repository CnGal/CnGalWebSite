using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.TimedTask.DataReositories;
using CnGalWebSite.TimedTask.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using CnGalWebSite.EventBus.Extensions;
using CnGalWebSite.TimedTask.Services;
using System.Text.Json.Serialization;
using CnGalWebSite.TimedTask.Extentions;
using CnGalWebSite.HealthCheck.Models;
using NLog;
using NLog.Web;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

// Add services to the container.
//添加数据库连接池
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseMySql(builder.Configuration["DefaultDBConnection"], ServerVersion.AutoDetect(builder.Configuration["DefaultDBConnection"]),
        o =>
        {
            //全局配置查询拆分模式
            o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            // 在查询中使用表达式包装集合
            o.TranslateParameterizedCollectionsToConstants();
        }));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}); ;
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//注册Swagger生成器，定义一个或多个Swagger文件
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CnGal资料站 - 定时任务 API",
        Description = "我们欢迎开发者使用这些API开发各个平台应用，如有困难请咨询网站管理人员",
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
builder.Services.AddHttpClient();
//添加自定义服务
builder.Services.AddScoped<IQueryService, QueryService>();
builder.Services.AddScoped<ITimedTaskService, TimedTaskService>();
builder.Services.AddEventBus();
//添加后台定时任务
builder.Services.AddHostedService<BackgroundTask>();
//注入HttpContext
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//添加状态检查
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("DbContext");
//依赖注入仓储
builder.Services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));
//添加OpenId 身份验证
//添加身份验证服务
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Authority"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

//添加授权范围
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "TaskAPI");
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(options =>
{
    options.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});

//添加状态检查终结点
app.UseHealthChecks("/healthz", ServiceStatus.Options);

//添加身份验证中间件
app.UseAuthentication();

//添加账户中间件
app.UseAuthorization();

app.MapControllers().RequireAuthorization("ApiScope");

app.Run();
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
