using Blazored.LocalStorage;
using CnGalWebSite.DataModel.Application.Examines;
using CnGalWebSite.DataModel.Application.Helper;
using CnGalWebSite.DataModel.Application.Roles;
using CnGalWebSite.DataModel.ViewModel.Files.Images;
using CnGalWebSite.Shared.Provider;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebView.Maui;

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

            builder.Services.AddBlazorWebView();

            builder.Services.AddBootstrapBlazor();

            builder.Services.AddScoped(sp => new HttpClient());


            // 增加 Table Excel 导出服务
            builder.Services.AddBootstrapBlazorTableExcelExport();

            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped(typeof(IPageModelCatche<>), typeof(PageModelCatche<>));
            builder.Services.AddScoped<IDataCacheService, DataCatcheService>();

            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IAppHelper, AppHelper>();
            builder.Services.AddScoped(x => new ExamineService());
            builder.Services.AddScoped(x => new ImagesLargeViewService());
            builder.Services.AddMasaBlazor();
            return builder.Build();
        }
    }
}
