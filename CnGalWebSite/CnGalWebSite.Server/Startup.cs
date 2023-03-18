using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.HealthCheck.Checks;
using CnGalWebSite.HealthCheck.Models;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            //覆盖默认api地址
            if (string.IsNullOrWhiteSpace(Configuration["WebApiPath"]) == false)
            {
                ToolHelper.WebApiPath = Configuration["WebApiPath"];
            }
            //本地化
            services.AddLocalization()
                .AddBootstrapBlazor()
                .AddScoped(sp => new HttpClient())
                .AddBlazoredLocalStorage()
                .AddBlazoredSessionStorage()
                .AddAuthorizationCore()
                .AddScoped<IHttpService, HttpService>()
                .AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>))
                .AddScoped<IDataCacheService, DataCatcheService>()
                .AddScoped(x => new ImagesLargeViewService())
                .AddMasaBlazor(s => s.ConfigureTheme(s =>
                {
                    s.Themes.Light.Primary = "#f06292";
                    s.Themes.Dark.Primary = "#2196F3";
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


            //默认采用cookie认证方案，添加oidc认证方案
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                //配置cookie认证
                .AddCookie("cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    //id4服务的地址
                    options.Authority = Configuration["Authority"];

                    //id4配置的ClientId以及ClientSecrets
                    options.ClientId = Configuration["ClientId"];
                    options.ClientSecret = Configuration["ClientSecret"];

                    //认证模式
                    options.ResponseType = "code";
                    //保存token到本地
                    options.SaveTokens = true;
                    //很重要，指定从Identity Server的UserInfo地址来取Claim
                    options.GetClaimsFromUserInfoEndpoint = true;
                    //指定要取哪些资料（除Profile之外，Profile是默认包含的）
                    options.Scope.Add("role");
                    options.Scope.Add("CnGalAPI");
                    //这里是个ClaimType的转换，Identity Server的ClaimType和Blazor中间件使用的名称有区别，需要统一。
                    options.TokenValidationParameters.RoleClaimType = "role";
                    options.TokenValidationParameters.NameClaimType = "name";
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

            //设置Cookies
            static void CheckSameSite(HttpContext httpContext, CookieOptions options)
            {
                if (options.SameSite == SameSiteMode.None)
                {
                    var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

                    options.SameSite = SameSiteMode.Unspecified;

                }
            }
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
