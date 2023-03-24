using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using System;

namespace CnGalWebSite.DataModel.ViewModel.Theme
{
    public class ThemeModel
    {
        public static string DefaultTheme = "pink lighten-2";
        public string Theme { get; set; } = DefaultTheme;

        public bool IsDark { get; set; }

        /// <summary>
        /// 是否背景透明
        /// </summary>
        public bool IsTransparent { get; set; } = true;

        /// <summary>
        /// 是否扩展到整个屏幕显示 不保存
        /// </summary>
        public bool IsExtendEntireScreen;

        /// <summary>
        /// 是否全屏显示 不留白边 官网使用
        /// </summary>
        public bool IsFullScreen { get; set; }

        /// <summary>
        /// 是否隐藏文章目录
        /// </summary>
        public bool IsHiddeArticleCatalog { get; set; }

        public bool IsCommentUseMarkdown { get; set; }

        /// <summary>
        /// 游玩记录字数提醒次数
        /// </summary>
        public int PlayedGameInforCount { get; set; } = 3;

        public DisplayMode ListDisplayMode { get; set; }

        /// <summary>
        /// 词条样式模板
        /// </summary>
        public EntryStyleTemplate EntryTemplate { get; set; }

        public AnniversariesSetting AnniversariesSetting { get; set; } = new AnniversariesSetting();

        public DateTime LastDisplayBrithdayTime { get; set; }

        public string FontFamily { get; set; } = "Helvetica,Tahoma,Arial,PingFang SC,Hiragino Sans GB,Heiti,Microsoft YaHei,WenQuanYi Micro Hei,sans-serif";

        /// <summary>
        /// 显示错误详细信息
        /// </summary>
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
