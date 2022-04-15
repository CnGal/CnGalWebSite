using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace CnGalWebSite.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            blazorWebView.UrlLoading +=
                (sender, urlLoadingEventArgs) =>
                {
                    urlLoadingEventArgs.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView;
                };
        }
    }
}
