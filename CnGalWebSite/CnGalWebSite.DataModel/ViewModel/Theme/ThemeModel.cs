using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using System;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Theme
{
    public class ThemeModel
    {
        public static string DefaultThemeColor = "pink";
        public static string[] AvailableColors = new string[] { "red", "pink", "purple", "deep-purple", "indigo", "blue", "light-blue", "cyan", "teal", "green", "light-green", "lime", "yellow", "amber", "orange", "deep-orange", "brown", "blue-grey", "grey" };

        /// <summary>
        /// 主题颜色
        /// </summary>
        public string ThemeColor { get; set; } = DefaultThemeColor;

        /// <summary>
        /// 主题模式
        /// </summary>
        public ThemeMode ThemeMode { get; set; }

        /// <summary>
        /// 最终是否为深色模式
        /// </summary>
        public bool IsDark;

        /// <summary>
        /// 是否背景透明
        /// </summary>
        public bool IsTransparent { get; set; } = true;

        /// <summary>
        /// 是否扩展到整个屏幕显示
        /// </summary>
        public bool IsExtendEntireScreen { get; set; }

        /// <summary>
        /// 是否全屏显示 不留白边 官网使用
        /// </summary>
        public bool IsFullScreen { get; set; }

        /// <summary>
        /// 是否隐藏文章目录
        /// </summary>
        public bool IsHiddeArticleCatalog { get; set; }

        /// <summary>
        /// 隐藏看板娘
        /// </summary>
        public bool HideKanban { get; set; }

        /// <summary>
        /// 使用Markdown编辑器编辑评论
        /// </summary>
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

        public string FontFamily { get; set; }

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

    public enum ThemeMode
    {
        [Display(Name ="跟随系统")]
        System,
        [Display(Name = "浅色")]
        Light,
        [Display(Name = "深色")]
        Dark,
    }

    public class AnniversariesSetting
    {
        public JudgableGamesSetting JudgableGamesSetting { get;set; }=new JudgableGamesSetting();
        public PlayedGameSetting PlayedGameSetting { get; set; } = new PlayedGameSetting();
    }

    public class JudgableGamesSetting
    {
        public string SearchString { get; set; }
        public JudgableGamesSortType SortType { get; set; }
        public JudgableGamesDisplayType DisplayType { get; set; }

        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 24;

        public int Count { get; set; } 

        public int TotalPages => (Count - 1) / MaxCount + 1;

        public int CurrentPage { get; set; } = 1;
    }

    public class PlayedGameSetting
    {
        public string SearchString { get; set; }
        public PlayedGamesSortType SortType { get; set; }
        public PlayedGamesDisplayType DisplayType { get; set; }

        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 24;

        public int Count { get; set; }

        public int TotalPages => (Count-1)/ MaxCount+1;

        public int CurrentPage { get; set; } = 1;
    }
}
