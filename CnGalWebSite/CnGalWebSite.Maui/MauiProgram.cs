using Blazored.LocalStorage;
using CnGalWebSite.DataModel.Application.Examines;
using CnGalWebSite.DataModel.Application.Helper;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.Maui.Services;
using CnGalWebSite.PublicToolbox.DataRepositories;
using CnGalWebSite.PublicToolbox.PostTools;
using CnGalWebSite.Shared.DataRepositories;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Components.Authorization;

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
            builder.Services.AddBlazoredLocalStorage();

            //身份验证
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            //缓存
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped<IAppHelper, AppHelper>();
            builder.Services.AddScoped(x => new ExamineService());
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


            return builder.Build();
        }
    }
}
