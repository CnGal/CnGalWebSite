using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.GameSite.API.DataReositories;
using CnGalWebSite.GameSite.API.Infrastructure;
using CnGalWebSite.GameSite.API.Services;
using CnGalWebSite.HealthCheck.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using NetCore.AutoRegisterDi;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
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

//注册Swagger生成器，定义一个或多个Swagger文件
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CnGal游戏站 - API",
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
}); builder.Services.AddHttpClient();


//自定义服务
//自动注入服务到依赖注入容器
builder.Services.RegisterAssemblyPublicNonGenericClasses()
   .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Provider"))
   .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);
//查询
builder.Services.AddScoped<IQueryService, QueryService>();
//HttpContext
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
//注入仓储
builder.Services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));
//添加HTTP请求
builder.Services.AddHttpClient();
builder.Services.AddScoped<IHttpService, HttpService>();

//添加真实IP
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

//添加状态检查
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("DbContext");

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
        policy.RequireClaim("scope", "GameAPI");
    });
});
var app = builder.Build();

app.UseDeveloperExceptionPage();

//添加真实IP中间件
app.UseForwardedHeaders();

app.UseSwagger(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});
app.UseSwaggerUI();

//添加状态检查终结点
app.UseHealthChecks("/healthz", ServiceStatus.Options);

//添加路由中间件
app.UseRouting();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseCors(options =>
{
    options.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});

//添加身份验证中间件
app.UseAuthentication();

//添加账户中间件
app.UseAuthorization();

app.MapControllers().RequireAuthorization("ApiScope");

app.Run();
