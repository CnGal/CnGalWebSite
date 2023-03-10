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
using System.Text;
using System.Text.Json;

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
                .AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>()
                .AddScoped<IAuthService, AuthService>()
                .AddScoped<IHttpService, HttpService>()
                .AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>))
                .AddScoped<IDataCacheService, DataCatcheService>()
                .AddScoped(x => new ImagesLargeViewService())
                .AddMasaBlazor(s => s.ConfigureTheme(s =>
                {
                    s.Themes.Light.Primary = "#f06292";
                    s.Themes.Dark.Primary = "#c9d1d9";
                }));

            //添加状态检查
            services.AddHealthChecks()
                //.AddCheck("API Server", new PingHealthCheck(ToolHelper.WebApiPath, 100))
                .AddCheck<SystemMemoryHealthcheck>("Memory");

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");
            //app.UseHttpsRedirection();
            //app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            //添加真实IP中间件
            _ = app.UseForwardedHeaders();

            _ = app.UseStaticFiles();

            //添加状态检查终结点
            _ = app.UseHealthChecks("/healthz", ServiceStatus.Options);

            //转发Ip
            _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            _ = app.UseRouting();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapBlazorHub();
                _ = endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
