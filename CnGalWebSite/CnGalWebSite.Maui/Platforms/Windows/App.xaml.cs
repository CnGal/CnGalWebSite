// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CnGalWebSite.Maui.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }


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