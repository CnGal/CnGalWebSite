using Blazored.LocalStorage;
using CnGalWebSite.DataModel.Application.Examines;
using CnGalWebSite.DataModel.Application.Helper;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Text;

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

            _ = services.AddServerSideBlazor()
                .AddHubOptions(options =>
                {
                    options.MaximumReceiveMessageSize = int.MaxValue;
                });


            _ = services.AddBootstrapBlazor();

            _ = services.AddScoped(sp => new HttpClient());

            _ = services.AddBlazoredLocalStorage();

            _ = services.AddAuthorizationCore();
            _ = services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            _ = services.AddScoped<IAuthService, AuthService>();
            _ = services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            _ = services.AddScoped<IDataCacheService, DataCatcheService>();

            _ = services.AddScoped<IAppHelper, AppHelper>();
            _ = services.AddScoped(x => new ExamineService());
            _ = services.AddScoped(x => new ImagesLargeViewService());
            _ = services.AddMasaBlazor();

            //添加状态检查
            _ = services.AddHealthChecks();

            //添加工具箱
            _ = services.AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IEntryService, EntryService>()
                .AddScoped<IArticleService, ArticleService>()
                .AddScoped<IImageService, ImageService>();
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

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
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
            _ = app.UseHealthChecks("/healthz");

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
