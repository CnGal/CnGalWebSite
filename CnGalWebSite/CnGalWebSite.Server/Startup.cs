using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.HealthCheck.Checks;
using CnGalWebSite.HealthCheck.Models;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Server.Plumbing;
using CnGalWebSite.Server.Services;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.Service;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //判断是否 SSR
            ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? true : ToolHelper.PreSetIsSSR.Value;
            //覆盖默认api地址
            if (string.IsNullOrWhiteSpace(Configuration["WebApiPath"]) == false)
            {
                ToolHelper.WebApiPath = Configuration["WebApiPath"];
            }
            if (string.IsNullOrWhiteSpace(Configuration["ImageApiPath"]) == false)
            {
                ToolHelper.ImageApiPath = Configuration["ImageApiPath"];
            }

            _ = services.AddRazorPages()
                 //设置Json格式化配置
                 .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
                });

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());

            _ = services.AddServerSideBlazor()
                .AddHubOptions(options =>
                {
                    options.MaximumReceiveMessageSize = int.MaxValue;
                });

            //本地化
            services.AddLocalization()
                .AddBootstrapBlazor()
                .AddBlazoredLocalStorage()
                .AddBlazoredSessionStorage()
                .AddAuthorizationCore()
                .AddScoped<IHttpService, HttpService>()
                .AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>))
                .AddScoped<IDataCacheService, DataCatcheService>()
                .AddScoped(x => new ImagesLargeViewService())
                .AddMasaBlazor(s => s.ConfigureTheme(s =>
                {
                    if(DateTime.Now.ToCstTime().Day==1&& DateTime.Now.ToCstTime().Month == 4)
                    {
                        s.Themes.Light.Primary = "#4CAF50";
                    }
                    else
                    {
                        s.Themes.Light.Primary = "#f06292";
                    }        
                    s.Themes.Dark.Primary = "#0078BF";
                }));

            //添加状态检查
            services.AddHealthChecks();

            //添加工具箱
            _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IEntryService, EntryService>()
                .AddScoped<IArticleService, ArticleService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IVideoService, VideoService>();
            //services.AddScoped<IEventBase, EventBase>();

            //添加预渲染状态记录
            _ = services.AddScoped<IApplicationStateService, ApplicationStateService>();
            //添加真实IP
            _ = services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            //添加结构化数据
            _ = services.AddScoped<IStructuredDataService, StructuredDataService>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //添加空白MAUI服务 
            services.AddScoped<IMauiService, MauiService>();

            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IEventService, EventService>();

            //自定义服务
            services.AddScoped<IHttpService, HttpService>();
            services.AddSingleton<ITokenStoreService, TokenStoreService>();
            //使用 IdentityModel 管理刷新令牌
            services.AddAccessTokenManagement();

            // not allowed to programmatically use HttpContext in Blazor Server.
            // that's why tokens cannot be managed in the login session
            services.AddScoped<IUserAccessTokenStore, ServerSideTokenStore>();

            // registers HTTP client that uses the managed user access token
            services.AddHttpClient<IHttpService, HttpService>(client =>
            {
                client.BaseAddress = new Uri(ToolHelper.WebApiPath);
            });

            //添加认身份证
            //services.AddAuthorization(options =>
            //{
            //    // By default, all incoming requests will be authorized according to the default policy
            //    // comment out if you want to drive the login/logout workflow from the UI
            //    options.FallbackPolicy = options.DefaultPolicy;
            //});
            //注册Cookie服务
            services.AddTransient<CookieEvents>();
            services.AddTransient<OidcEvents>();

            //默认采用cookie认证方案，添加oidc认证方案
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookies";
                options.DefaultChallengeScheme = "cngal";
                options.DefaultSignOutScheme = "cngal";
            })
                .AddCookie("cookies", options =>
                {
                    options.Cookie.Name = "__Host-blazor";
                    options.Cookie.SameSite = SameSiteMode.Lax;

                    options.EventsType = typeof(CookieEvents);
                })
                .AddOpenIdConnect("cngal", options =>
                {
                    //id4服务的地址
                    options.Authority = Configuration["Authority"];

                    //id4配置的ClientId以及ClientSecrets
                    options.ClientId = Configuration["ClientId"];
                    options.ClientSecret = Configuration["ClientSecret"];
                    //不检查Https
                    options.RequireHttpsMetadata = false;

                    //认证模式
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    //保存token到本地
                    options.SaveTokens = true;
                    //很重要，指定从Identity Server的UserInfo地址来取Claim
                    options.GetClaimsFromUserInfoEndpoint = true;
                    //指定要取哪些资料（除Profile之外，Profile是默认包含的）
                    options.Scope.Add("role");
                    options.Scope.Add("openid");
                    options.Scope.Add("CnGalAPI");
                    options.Scope.Add("FileAPI");
                    options.Scope.Add("offline_access");
                    //这里是个ClaimType的转换，Identity Server的ClaimType和Blazor中间件使用的名称有区别，需要统一。
                    options.TokenValidationParameters.RoleClaimType = "role";
                    options.TokenValidationParameters.NameClaimType = "name";
                    //注册事件
                    options.EventsType = typeof(OidcEvents);


                    options.Events.OnUserInformationReceived = (context) =>
                    {
                        //回顾之前关于WebAssembly的例子，涉及到数组的转换，这里也一样要处理

                        ClaimsIdentity claimsId = context.Principal.Identity as ClaimsIdentity;

                        var roleElement = context.User.RootElement.GetProperty("role");
                        if (roleElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var roles = context.User.RootElement.GetProperty("role").EnumerateArray().Select(e =>
                            {
                                return e.ToString();
                            });
                            claimsId.AddClaims(roles.Select(r => new Claim("role", r)));
                        }
                        else
                        {
                            claimsId.AddClaim(new Claim("role", roleElement.ToString()));
                        }


                        return Task.CompletedTask;
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //设置请求来源
            if (!env.IsDevelopment())
            {
                app.Use((context, next) =>
                {
                    context.Request.Scheme = "https";
                    return next();
                });
            }
            _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");
            //app.UseHttpsRedirection();
            //app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            //转发Ip
            _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //设置Cookies
            app.UseCookiePolicy();

            _ = app.UseStaticFiles();

            //添加状态检查终结点
            _ = app.UseHealthChecks("/healthz", ServiceStatus.Options);

            _ = app.UseRouting();

            //添加认证与授权中间件
            app.UseAuthentication();
            app.UseAuthorization();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapDefaultControllerRoute();
                _ = endpoints.MapBlazorHub();
                _ = endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
