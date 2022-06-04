namespace CnGalWebSite.DataModel.ViewModel.Theme
{
    public class ThemeModel
    {
        public string Theme { get; set; } = "pink lighten-2";

        public ThemeType Type { get; set; }

        public bool IsDark { get; set; }

        public bool IsDebug { get; set; }

        public bool IsTransparent { get; set; } = true;

        public bool IsMasa { get; set; } = true;

        public int PlayedGameInforCount { get; set; } = 3;

        public DisplayMode ListDisplayMode { get; set; }
    }

    public enum DisplayMode
    {
        Image,
        Text
    }

    public enum ThemeType
    {
        Default,
        Custom,
    }
}
