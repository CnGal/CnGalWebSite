namespace CnGalWebSite.DataModel.ViewModel.Theme
{
    public class ThemeModel
    {
        public bool UseTabSet { get; set; }

        public string Theme { get; set; } = "pink lighten-2";

        public  ThemeType Type { get; set; }

        public bool IsOpen { get; set; }

        public bool IsFixedHeader { get; set; } = true;

        public bool IsFixedFooter { get; set; } 

        public bool IsFullSide { get; set; } = true;

        public bool ShowFooter { get; set; } = true;

        public bool IsDark { get; set; } 

        public bool IsDebug { get; set; }

        public bool IsOnMouse { get; set; }

        public bool IsOnBgImage { get; set; }

        public bool IsMasa { get; set; } = true;

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
