using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.MasaComponent.Shared.Tips;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.WebAssembly
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            //判断是否 SSR
            ToolHelper.IsSSR = ToolHelper.PreSetIsSSR == null ? false : ToolHelper.PreSetIsSSR.Value;

            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.RootComponents.Add<App>("app");

            // 增加 BootstrapBlazor 组件
            builder.Services.AddBootstrapBlazor();
            //动态修改标题
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddBlazoredLocalStorage()
                 .AddBlazoredSessionStorage();

            builder.Services.AddScoped<IHttpService, HttpService>();
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped(x => new ImagesLargeViewService());

            builder.Services.AddMasaBlazor(s => s.ConfigureTheme(s =>
            {
                s.Themes.Light.Primary = "#f06292";
                s.Themes.Dark.Primary = "#2196F3";
            }));

            //添加工具箱
            _ = builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>))
                .AddScoped<IEntryService, EntryService>()
                .AddScoped<IArticleService, ArticleService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IVideoService, VideoService>();
            //添加预渲染状态记录
            builder.Services.AddScoped<IApplicationStateService, ApplicationStateService>();

            //添加结构化数据
            builder.Services.AddScoped<IStructuredDataService, StructuredDataService>();
            builder.RootComponents.Add<StructuredDataTip>("head::after");

            builder.Services.BuildServiceProvider(validateScopes: false);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());

            //添加空白MAUI服务 
            builder.Services.AddScoped<IMauiService, MauiService>();

            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IEventService, EventService>();

            //添加OpenId
            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Local", options.ProviderOptions);
                options.UserOptions.RoleClaim = "role";
                options.UserOptions.NameClaim = "name";
            }).AddAccountClaimsPrincipalFactory<CustomUserFactory>();

            //自定义消息
            //builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            //builder.Services.AddHttpClient("CnGalAPI")
            //    //声明使用以上自定义的处理程序
            //    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
            //builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
            //    .CreateClient("CnGalAPI"));
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
