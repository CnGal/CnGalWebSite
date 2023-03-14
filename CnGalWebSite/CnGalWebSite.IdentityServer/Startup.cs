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
using CnGalWebSite.IdentityServer.Models.Account;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using NetCore.AutoRegisterDi;
using NewCngal.CustomMiddlewares;
using CnGalWebSite.APIServer.DataReositories;

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

            //设置密码格式要求
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.User.AllowedUserNameCharacters = null;
                options.User.RequireUniqueEmail = true;
            });

            //添加HTTP客户端
            services.AddHttpClient();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
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
            })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    
                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    options.ClientId = Configuration["GoogleClientId"];
                    options.ClientSecret = Configuration["GoogleClientSecret"];
                });

            services.AddDatabaseDeveloperPageExceptionFilter();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }

            //转发Ip
            _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
