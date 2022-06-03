using Blazored.LocalStorage;
using CnGalWebSite.DataModel.Application.Examines;
using CnGalWebSite.DataModel.Application.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
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
            services.AddRazorPages();
            services.AddServerSideBlazor()
                .AddHubOptions(options =>
                {
                    options.MaximumReceiveMessageSize = int.MaxValue;
                });

            services.AddBootstrapBlazor();

            services.AddScoped(sp => new HttpClient());


            // 增加 Table Excel 导出服务
            services.AddBootstrapBlazorTableExcelExport();

            services.AddBlazoredLocalStorage();

            services.AddAuthorizationCore();
            services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            services.AddScoped<IDataCacheService, DataCatcheService>();

            services.AddScoped<IAppHelper, AppHelper>();
            services.AddScoped(x => new ExamineService());
            services.AddScoped(x => new ImagesLargeViewService());
            services.AddMasaBlazor();

            //添加状态检查
            services.AddHealthChecks();

            //添加工具箱
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IEntryService, EntryService>();
            services.AddScoped<IArticleService, ArticleService>();
            services.AddScoped<IImageService, ImageService>();
            //services.AddScoped<IEventBase, EventBase>();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            //app.UseHttpsRedirection();
            //app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);

            app.UseStaticFiles();

            //添加状态检查终结点
            app.UseHealthChecks("/healthz");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
