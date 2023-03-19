// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using CnGalWebSite.IdentityServer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using NetCore.AutoRegisterDi;
using NewCngal.CustomMiddlewares;
using CnGalWebSite.APIServer.DataReositories;
using System;
using Microsoft.AspNetCore.Authentication.QQ;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //设置地址
            Config.IdsSSR = Configuration["IdsSSR"];
            Config.IdsWASM = Configuration["IdsWASM"];
            Config.SSR = Configuration["SSR"];
            Config.WASM = Configuration["WASM"];
            //添加状态检查
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("DbContext");
            //MVC
            services.AddControllersWithViews();

            //添加数据库
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration["DefaultDBConnection"], ServerVersion.AutoDetect(Configuration["DefaultDBConnection"]),
                    o =>
                    {
                        //全局配置查询拆分模式
                        o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }));

            //依赖注入仓储
            services.AddTransient(typeof(IRepository<,>), typeof(RepositoryBase<,>));

            //添加 MailKit 发送邮件
            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(new MailKitOptions()
                {
                    //get options from sercets.json
                    Server = Configuration["Server"],
                    Port = int.Parse(Configuration["Port"]),
                    SenderName = Configuration["SenderName"],
                    SenderEmail = Configuration["SenderEmail"],

                    // can be optional with no authentication 
                    Account = Configuration["Account"],
                    Password = Configuration["Password"],
                    // enable ssl or tls
                    Security = true
                });
            });

            //自动注入服务到依赖注入容器
            services.RegisterAssemblyPublicNonGenericClasses()
               .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Provider"))
               .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            //账户设置
            services.Configure<IdentityOptions>(options =>
            {
                //密码格式要求
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                //用户名
                options.User.AllowedUserNameCharacters = null;
                //唯一电子邮箱
                options.User.RequireUniqueEmail = true;
                //账户锁定
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            });

            //添加HTTP客户端
            services.AddHttpClient();

            //为本地 API 启用令牌验证
            services.AddLocalApiAuthentication();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;

                //添加本地API到发现文档
                options.Discovery.CustomEntries.Add("local_api", "~/api");
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();

            //设置证书
            if (string.IsNullOrWhiteSpace(Configuration["CertPath"]) || string.IsNullOrWhiteSpace(Configuration["CertPassword"]))
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                builder.AddSigningCredential(new X509Certificate2(Configuration["CertPath"], Configuration["CertPassword"]));
            }


            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = Configuration["GoogleClientId"];
                    options.ClientSecret = Configuration["GoogleClientSecret"];
                })
                .AddGitHub(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = Configuration["GitHubClientId"];
                    options.ClientSecret = Configuration["GitHubClientSecret"];
                })

                .AddQQ(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = Configuration["QQClientId"];
                    options.ClientSecret = Configuration["QQClientSecret"];
                })
                .AddGitee(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = Configuration["GiteeClientId"];
                    options.ClientSecret = Configuration["GiteeClientSecret"];
                });

            //API终结点
            builder.Services.AddEndpointsApiExplorer();
            //注册Swagger生成器，定义一个或多个Swagger文件
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "CnGal资料站 - 开放平台 API",
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

            //添加真实IP
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

        }

        public void Configure(IApplicationBuilder app)
        {
            _ = Environment.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");

            //转发Ip
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
            //跨域策略
            app.UseCors(options =>
            {
                options.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
