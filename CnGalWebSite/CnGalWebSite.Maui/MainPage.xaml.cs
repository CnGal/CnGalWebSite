using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace CnGalWebSite.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {

            InitializeComponent();

            blazorWebView.ExternalNavigationStarting +=
                (sender, externalLinkNavigationEventArgs) =>
                {
                    externalLinkNavigationEventArgs.ExternalLinkNavigationPolicy = ExternalLinkNavigationPolicy.InsecureOpenInWebView;
                };

        }
    }
}
