using CnGalWebSite.Maui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using System.Diagnostics.CodeAnalysis;
using CnGalWebSite.DataModel.ViewModel.Others;

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

        public override void Quit()
        {
#if ANDROID
            var applicationService = new ApplicationService();
            applicationService.CloseApplication();
#endif
        }

        public override void ThemeChanged(string theme)
        {
            _overviewService.HideLoadingOverview();
#if ANDROID
            var themeService = new ThemeService();
            themeService.SetStatusBarColor(Color.FromArgb(theme));
#endif

        }

        public override async void ShareLink(ShareLinkModel model)
{
            await Share.RequestAsync(new ShareTextRequest
            {
                Uri = model.Link,
                Text=model.Text,
                Title = model.Title
            });
        }
    }
}
