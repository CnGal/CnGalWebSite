using AspNetCoreRateLimit;
using CnGalWebSite.APIServer.Application.BackgroundTasks;
using CnGalWebSite.APIServer.Application.ElasticSearches;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.DataModel.Model;
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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NetCore.AutoRegisterDi;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
                options.UseMySql(Configuration.GetConnectionString("CnGalDBConnection"), ServerVersion.AutoDetect(Configuration.GetConnectionString("CnGalDBConnection")),
                    o =>
                    {
                        //全局配置查询拆分模式
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }).UseBatchEF_MySQLPomelo());
            // services.AddMvc(async => async.EnableEndpointRouting = false);
            //不使用终结点路由
            services.AddControllersWithViews(a => a.EnableEndpointRouting = false);
            //依赖注入辅助类
            services.AddScoped<IAppHelper, AppHelper>();
            services.AddScoped<IRSSHelper, RSSHelper>();
            //添加HTTP请求
            services.AddHttpClient();
            //添加ElasticSearch服务
            services.AddTransient<IElasticsearchProvider, ElasticsearchProvider>();
            services.AddTransient(typeof(IElasticsearchBaseService<>), typeof(ElasticsearchBaseService<>));
            //依赖注入仓储
            services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));
            //自动注入服务到依赖注入容器
            services.RegisterAssemblyPublicNonGenericClasses()
               .Where(c => c.Name.EndsWith("Service"))
               .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            //注册Swagger生成器，定义一个或多个Swagger文件
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CnGal API",
                    Description = "我们欢迎开发者使用这些API开发各个平台应用，如有困难请咨询网站管理人员",
                    Contact = new OpenApiContact
                    {
                        Name = "沙雕の方块",
                        Email = "1278490989@qq.com"
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

            //添加身份验证
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<CustomMiddlewares.CustomIdentityErrorDescriber>()
                .AddEntityFrameworkStores<AppDbContext>();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.User.AllowedUserNameCharacters = null;
            });

            //注册JWT令牌验证
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidAudience = Configuration["JwtAudience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"]))
                    };
                });
            services.AddControllersWithViews(config =>
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            }).AddXmlSerializerFormatters();


            services.Configure<IdentityOptions>(options =>
            {
                //登入时需要验证电子邮件
                options.SignIn.RequireConfirmedEmail = true;
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
            services.AddApplicationInsightsTelemetry();

            //添加限流
            // needed to load configuration from appsettings.json
            services.AddOptions();

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddInMemoryRateLimiting();
            //services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
            //services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            //services.AddRedisRateLimiting();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            //添加后台定时任务
            services.AddHostedService<BackgroundTask>();
            //添加真实IP
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            //添加状态检查
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //添加限流中间件
            app.UseIpRateLimiting();
            /*   if (env.IsDevelopment())
               {*/
            app.UseDeveloperExceptionPage();
            /* }
             else
             {
                 app.UseExceptionHandler("/Error");
                 app.UseStatusCodePagesWithReExecute("/Error/{0}");
             }*/
            //添加真实IP中间件
            app.UseForwardedHeaders();
            //添加HTTPS中间件
            //app.UseHttpsRedirection();
            //添加静态文件中间件
            app.UseStaticFiles();
            //启用中间件Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CnGal API V1");
            });

            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);
            //添加状态检查终结点
            app.UseHealthChecks("/healthz");

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

            //添加终结点
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=swagger}/{action=Index}/{id?}");
            });/* app.Run(ctx =>
             {
                 ctx.Response.Redirect("/swagger/"); //可以支持虚拟路径或者index.html这类起始页.
                return Task.FromResult(0);
             });*/
        }
    }
}
