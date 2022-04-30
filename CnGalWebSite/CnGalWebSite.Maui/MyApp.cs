
using CnGalWebSite.Maui.Platforms.Android.Services;
using CnGalWebSite.Maui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Diagnostics.CodeAnalysis;

namespace CnGalWebSite.Maui
{
    /// <summary>
    /// 
    /// </summary>
   public class MyApp : CnGalWebSite.Shared.App
    {

        [Inject]
        [NotNull]
        private IAlertService _alertService { get; set; }
        [Inject]
        [NotNull]
        private IOverviewService _overviewService { get; set; }


        public override void ShowAlert()
        {
            _alertService.ShowAlert("Blazor通信测试", "Blazor弹窗测试");
        }

        public override async void OpenNewPage(string url)
        {
            //_serviceProvider.GetService()
            await Browser.OpenAsync(url, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.External,
                TitleMode = BrowserTitleMode.Show
            });
        }

        public override void Loaded()
        {

        }

        public override void ThemeChanged(string theme)
        {
            _overviewService.HideLoadingOverview();

            ThemeService themeService = new ThemeService();
            themeService.SetStatusBarColor(Color.FromArgb(theme));


        }
    }
}
