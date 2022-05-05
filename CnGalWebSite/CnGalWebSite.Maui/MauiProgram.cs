using CnGalWebSite.Maui.Services;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;

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

            // 增加 Table Excel 导出服务
            builder.Services.AddBootstrapBlazorTableExcelExport();
            //本地储存服务
            builder.Services.AddBlazoredLocalStorage();

            //身份验证
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            //缓存
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IAppHelper, AppHelper>();
            builder.Services.AddScoped(x => new ExamineService());
            builder.Services.AddScoped(x => new ImagesLargeViewService());

            //MASA组件库
            builder.Services.AddMasaBlazor();

            //弹窗服务类
            builder.Services.AddSingleton(typeof(IAlertService), typeof(AlertService));
            builder.Services.AddSingleton(typeof(IOverviewService), typeof(OverviewService));


            return builder.Build();
        }
    }
}
