using CnGalWebSite.Maui.Platforms.Android.Services;
using CnGalWebSite.Maui.Services;

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
                    urlLoadingEventArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenExternally;
                };
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

            var themeService = new ThemeService();
            themeService.SetStatusBarColor(Color.FromArgb("#FFFFFF"));
        }

        public void HideOverviewGrid()
        {
            OverviewGrid.IsVisible = false;
        }
    }
}
