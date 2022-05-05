
/* 项目“CnGalWebSite.Maui (net6.0-windows10.0.19041)”的未合并的更改
在此之前:
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility;
using Android.Content;
using Android.Runtime;
using Plugin.CurrentActivity;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
在此之后:
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Plugin.CurrentActivity;
*/
namespace CnGalWebSite.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


        }


    }
}
