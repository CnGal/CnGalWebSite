
using CnGalWebSite.Maui.Services;
using Microsoft.AspNetCore.Components.WebView;

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

            Loaded += MainPage_LoadedAsync;
        }

        private async void MainPage_LoadedAsync(object sender, EventArgs e)
        {
            //_alertService.ShowAlert("测试","弹窗测试");
            //await Browser.OpenAsync("https://www.cngal.org/", new BrowserLaunchOptions
            //{
            //    LaunchMode = BrowserLaunchMode.SystemPreferred,
            //    TitleMode = BrowserTitleMode.Show
            //});

#if ANDROID
            ThemeService.SetStatusBarColor(Color.FromArgb("#FFFFFF"));
#endif

        }

        public void HideOverviewGrid()
        {
            OverviewGrid.IsVisible = false;
        }
    }
}
