﻿using Foundation;

namespace CnGalWebSite.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {

        /* 项目“CnGalWebSite.Maui (net6.0-windows10.0.19041)”的未合并的更改
        在此之前:
                protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
        在此之后:
                protected override MauiApp CreateMauiApp()
                {
                    return MauiProgram.CreateMauiApp();
        */
        protected override MauiApp CreateMauiApp()
        {
            return MauiProgram.CreateMauiApp();
        }
    }
}
