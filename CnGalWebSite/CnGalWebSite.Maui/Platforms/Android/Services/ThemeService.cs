using Android.OS;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Platform = Microsoft.Maui.ApplicationModel.Platform;

namespace CnGalWebSite.Maui.Services
{
    public partial class ThemeService
    {
        public void SetStatusBarColor(Color color)
        {
            // The SetStatusBarcolor is new since API 21
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
    }
}
