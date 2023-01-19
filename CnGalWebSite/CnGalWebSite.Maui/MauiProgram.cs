using Blazored.LocalStorage;
using Blazored.SessionStorage;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.Maui.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Extentions;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Components.Authorization;
using MauiService = CnGalWebSite.Maui.Services.MauiService;

namespace CnGalWebSite.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            builder.Services.AddBootstrapBlazor();

            builder.Services.AddScoped(sp => new HttpClient());

            //依赖注入主页
            builder.Services.AddSingleton<MainPage>();
            //本地储存服务
            builder.Services.AddBlazoredLocalStorage()
                .AddBlazoredSessionStorage();

            //身份验证
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            //缓存
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped(x => new ImagesLargeViewService());

            //添加工具箱
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IEntryService, EntryService>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IImageService, ImageService>();
            //services.AddScoped<IEventBase, EventBase>();

            //添加预渲染状态记录
            builder.Services.AddScoped<IApplicationStateService, ApplicationStateService>();

            //添加结构化数据
            builder.Services.AddScoped<IStructuredDataService, StructuredDataService>();

            //MASA组件库
            builder.Services.AddMasaBlazor();

            //弹窗服务类
            builder.Services.AddSingleton(typeof(IAlertService), typeof(AlertService));
            builder.Services.AddSingleton(typeof(IOverviewService), typeof(OverviewService));

            //设置Json格式化配置
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            ToolHelper.options.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());

            //添加MAUI与Blazor通信服务
            builder.Services.AddScoped<IMauiService, MauiService>();

            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IEventService, EventService>();


            return builder.Build();
        }
    }
}
