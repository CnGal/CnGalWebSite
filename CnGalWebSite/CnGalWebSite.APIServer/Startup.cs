using CnGalWebSite.APIServer.Application.BackgroundTasks;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Typesense;
using CnGalWebSite.APIServer.CustomMiddlewares;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Extentions;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.HealthCheck.Checks;
using CnGalWebSite.HealthCheck.Models;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetCore.AutoRegisterDi;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Typesense.Setup;
using CnGalWebSite.EventBus.Extensions;
using CnGalWebSite.APIServer.Application.Tasks;

namespace CnGalWebSite.APIServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //添加数据库连接池
            services.AddDbContextPool<AppDbContext>(options =>
                options.UseMySql(Configuration["CnGalDBConnection"], ServerVersion.AutoDetect(Configuration["CnGalDBConnection"]),
                    o =>
                    {
                        //全局配置查询拆分模式
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        // 在查询中使用表达式包装集合
                        o.TranslateParameterizedCollectionsToConstants();
                    }));

            //配置Json
            services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            ToolHelper.options.Converters.Add(new JsonStringEnumConverter());

            //自动注入服务到依赖注入容器
            services.RegisterAssemblyPublicNonGenericClasses()
               .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Provider"))
               .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);
            //依赖注入辅助类
            services.AddScoped<IAppHelper, AppHelper>();
            services.AddScoped<IRSSHelper, RSSHelper>();
            //添加HTTP请求
            services.AddHttpClient();
            services.AddScoped<IHttpService, HttpService>();
            //添加ElasticSearch服务
            //services.AddTransient(typeof(IElasticsearchBaseService<>), typeof(ElasticsearchBaseService<>));
            //添加搜索服务
            services.AddScoped<ISearchHelper, TypesenseHelper>()
                .AddTypesenseClient(config =>
                {
                    config.ApiKey = Configuration["TypesenseAPIKey"];
                    config.Nodes = new List<Node> { new Node(Configuration["TypesenseHost"], Configuration["TypesensePort"]) };
                });
            //依赖注入仓储
            services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));
            //添加文件上传服务
            services.AddScoped<IFileUploadService, FileUploadService>();
            //添加Query
            services.AddScoped<IQueryService, QueryService>();
            //添加内存缓存
            services.AddMemoryCache();

            //注册Swagger生成器，定义一个或多个Swagger文件
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CnGal资料站 - 主站 API",
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

            //添加OpenId 身份验证
            //添加身份验证服务
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["Authority"];
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false
                    };
                });

            //添加授权范围
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "CnGalAPI");
                });
            });

            //添加 MailKit 发送邮件
            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(new MailKitOptions()
                {
                    //get options from sercets.json
                    Server = Configuration["Server"],
                    Port = Convert.ToInt32(Configuration["Port"]),
                    SenderName = Configuration["SenderName"],
                    SenderEmail = Configuration["SenderEmail"],

                    // can be optional with no authentication
                    Account = Configuration["Account"],
                    Password = Configuration["Password"],
                    // enable ssl or tls
                    Security = true
                });
            });
            //事件总线
            services.AddEventBus();
            //添加后台定时任务
            services.AddHostedService<BackgroundTask>();
            services.AddSingleton<IBackgroundTaskService,BackgroundTaskService>();
            //添加真实IP
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            //添加HttpContext服务
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //添加状态检查
            services.AddHealthChecks().AddDbContextCheck<AppDbContext>("DbContext");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            //添加真实IP中间件
            app.UseForwardedHeaders();
            //添加HTTPS中间件
            //app.UseHttpsRedirection();
            //添加静态文件中间件
            app.UseStaticFiles();
            //启用中间件Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            //添加状态检查终结点
            app.UseHealthChecks("/healthz", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {

            });

            //添加路由中间件
            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //跨域策略
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

            //添加终结点
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireAuthorization("ApiScope");
            });
        }
    }
}
