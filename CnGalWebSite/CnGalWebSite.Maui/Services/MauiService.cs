using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.Shared.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Maui.Services
{
    public class MauiService:IMauiService
    {
        private readonly IAlertService _alertService;
        private readonly IOverviewService _overviewService;

        public MauiService(IAlertService alertService, IOverviewService overviewService)
        {
            _alertService = alertService;
            _overviewService = overviewService;
        }

        public void ChangeTheme(string theme)
        {
            ThemeService.SetStatusBarColor(Color.FromArgb(theme));
        }

        public void OnLoaded()
        {
            _overviewService.HideLoadingOverview();
        }

        public async Task ShareLink(ShareLinkModel model)
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Uri = model.Link,
                Text = model.Text,
                Title = model.Title
            });
        }

        public void Quit()
        {
            ApplicationService.CloseApplication();
        }

        public void SetStatusBarTransparent(bool hide)
        {
            ThemeService.SetStatusBarTransparent(hide);
        }

        public async Task OpenNewPage(string url)
        {
            //_serviceProvider.GetService()
            await Browser.OpenAsync(url, new BrowserLaunchOptions
            {
                LaunchMode = BrowserLaunchMode.External,
                TitleMode = BrowserTitleMode.Show
            });
        }
    }
}
