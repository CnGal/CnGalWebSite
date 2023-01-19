using Android.App;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

namespace CnGalWebSite.Maui.Services
{
    public partial class ThemeService
    {
        public static void SetStatusBarColor(Color color)
        {
            // The SetStatusBarColor is new since API 21
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var androidColor = color./*AddLuminosity((float)-0.1).*/ToAndroid();
                //Just use the plugin
                var activity = Platform.CurrentActivity;

                activity.Window.SetStatusBarColor(androidColor);

              
            }
            else
            {
                // Here you will just have to set your 
                // color in styles.xml file as shown below.
            }
        }

        public static void SetStatusBarTransparent(bool hide)
        {
            var activity = Platform.CurrentActivity;
            if (hide)
            {
                activity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);
                activity.Window.AddFlags(WindowManagerFlags.TranslucentNavigation);
            }
            else
            {
                activity.Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                activity.Window.ClearFlags(WindowManagerFlags.TranslucentNavigation);
            }
        }
    }
}
