using CnGalWebSite.DataModel.ViewModel.Anniversaries;

namespace CnGalWebSite.DataModel.ViewModel.Theme
{
    public class ThemeModel
    {
        public string Theme { get; set; } = "pink lighten-2";

        public ThemeType Type { get; set; }

        public bool IsDark { get; set; }

        public bool IsDebug { get; set; }

        /// <summary>
        /// 是否背景透明
        /// </summary>
        public bool IsTransparent { get; set; } = true;

        public bool IsCommentUseMarkdown { get; set; }

        public int PlayedGameInforCount { get; set; } = 3;

        public DisplayMode ListDisplayMode { get; set; }

        public AnniversariesSetting AnniversariesSetting { get; set; } = new AnniversariesSetting();
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

    public class AnniversariesSetting
    {
        public string SearchString { get; set; }
        public JudgableGamesSortType SortType { get; set; }
        public JudgableGamesDisplayType DisplayType { get; set; }

        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 24;

        public int Count { get; set; } 

        public int TotalPages => (Count / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;
    }
}
