﻿
using CnGalWebSite.Maui.Services;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace CnGalWebSite.Maui
{
    public partial class MainPage : ContentPage
    {
        private readonly IAlertService _alertService;
        private readonly IOverviewService _overviewService;

        public MainPage(IAlertService alertService, IOverviewService overviewService)
        {
            _alertService = alertService;
            _alertService.Init(this);

            _overviewService = overviewService;
            _overviewService.Init(this);

            InitializeComponent();

            blazorWebView.UrlLoading +=
            (sender, urlLoadingEventArgs) =>
            {
                if (urlLoadingEventArgs.Url.Host != "0.0.0.0")
                {
                    urlLoadingEventArgs.UrlLoadingStrategy =
                        UrlLoadingStrategy.OpenInWebView;
                }
            };
            Loaded += MainPage_LoadedAsync;
        }

        private void MainPage_LoadedAsync(object sender, EventArgs e)
        {
            //_alertService.ShowAlert("测试","弹窗测试");
            //await Browser.OpenAsync("https://www.cngal.org/", new BrowserLaunchOptions
            //{
            //    LaunchMode = BrowserLaunchMode.SystemPreferred,
            //    TitleMode = BrowserTitleMode.Show
            //});
            ThemeService.SetStatusBarColor(Color.FromArgb("#FFFFFF"));
        }

        public void HideOverviewGrid()
        {
            OverviewGrid.IsVisible = false;
        }
    }
}
