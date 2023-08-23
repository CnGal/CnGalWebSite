using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.DrawingBed.Helper.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.MasaComponent.Shared.Tips;
using CnGalWebSite.Shared.Service;
using CnGalWebSite.WebAssembly.Services;
using Masa.Blazor.Presets;
using Masa.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
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
            //覆盖默认api地址
            if (string.IsNullOrWhiteSpace(builder.Configuration["WebApiPath"]) == false)
            {
                ToolHelper.WebApiPath = builder.Configuration["WebApiPath"];
            }
            if (string.IsNullOrWhiteSpace(builder.Configuration["ImageApiPath"]) == false)
            {
                ToolHelper.ImageApiPath = builder.Configuration["ImageApiPath"];
            }

            builder.RootComponents.Add<App>("app");

            // 增加 BootstrapBlazor 组件
            builder.Services.AddBootstrapBlazor();
            //动态修改标题
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddBlazoredLocalStorage()
                 .AddBlazoredSessionStorage();

            
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped(x => new ImagesLargeViewService());

            builder.Services.AddMasaBlazor(options =>
            {
                //消息队列
                options.Defaults = new Dictionary<string, IDictionary<string, object?>?>()
                    {
                        {
                            PopupComponents.SNACKBAR, new Dictionary<string, object?>()
                            {
                                { nameof(PEnqueuedSnackbars.Outlined), true },
                                { nameof(PEnqueuedSnackbars.Closeable), true },
                                { nameof(PEnqueuedSnackbars.Position), SnackPosition.BottomRight }
                            }
                        }
                    };
                //主题
                options.ConfigureTheme(s =>
                {
                    if (DateTime.Now.ToCstTime().Day == 1 && DateTime.Now.ToCstTime().Month == 4)
                    {
                        s.Themes.Light.Primary = "#4CAF50";
                    }
                    else
                    {
                        s.Themes.Light.Primary = "#f06292";
                    }
                    s.Themes.Dark.Primary = "#0078BF";
                });
            });
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
            ToolHelper.options.Converters.Add(new JsonStringEnumConverter());

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
                options.ProviderOptions.ResponseMode = "query";
                options.ProviderOptions.ResponseType = "code";
            }).AddAccountClaimsPrincipalFactory<CustomUserFactory>();
            //添加Http服务
            builder.Services.AddSingleton<IHttpService, HttpService>();
            //注册身份验证的HttpClient
            builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            builder.Services.AddHttpClient("AuthAPI")
                .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

            //注册匿名的HttpClient
            builder.Services.AddHttpClient("AnonymousAPI");
            //添加Query
            builder.Services.AddScoped<IQueryService, QueryService>();
            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
