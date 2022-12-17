using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using System;

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

        /// <summary>
        /// 是否扩展到整个屏幕显示
        /// </summary>
        public bool IsExtendEntireScreen { get; set; }

        /// <summary>
        /// 是否隐藏文章目录
        /// </summary>
        public bool IsHiddeArticleCatalog { get; set; }

        public bool IsCommentUseMarkdown { get; set; }

        public int PlayedGameInforCount { get; set; } = 3;

        public DisplayMode ListDisplayMode { get; set; }

        public AnniversariesSetting AnniversariesSetting { get; set; } = new AnniversariesSetting();

        public DateTime LastDisplayBrithdayTime { get; set; }

        public string FontFamily { get; set; } = "Helvetica,Tahoma,Arial,PingFang SC,Hiragino Sans GB,Heiti SC,Microsoft YaHei,WenQuanYi Micro Hei,sans-serif";

        public bool ShowDetailedErrorInfor { get; set; }

        public bool AllowAutoSave { get; set; } = true;
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
