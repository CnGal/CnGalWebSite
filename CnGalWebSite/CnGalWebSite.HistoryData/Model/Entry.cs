﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
#else
#endif
namespace CnGalWebSite.DataModel.Model
{
    public class Entry
    {
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 别称
        /// </summary>
        public string AnotherName { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 类型
        /// </summary>
        public EntryType Type { get; set; }
        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool CanComment { get; set; } = true;
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;
        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 游戏发布时间 只对游戏词条有效
        /// </summary>
        public DateTime? PubulishTime { get; set; }

        /// <summary>
        /// 附加信息列表
        /// </summary>
        public ICollection<BasicEntryInformation> Information { get; set; }

        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(1000000)]
        public string MainPage { get; set; }

        /// <summary>
        /// 相关性列表
        /// </summary>
        public ICollection<EntryRelevance> Relevances { get; set; }

        /// <summary>
        /// 图片列表
        /// </summary>
        public ICollection<EntryPicture> Pictures { get; set; }
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; }

        public ICollection<Tag> Tags { get; set; }

        public string SteamId { get; set; }

    }

    public class EditionModel
    {
        public int Id { get; set; }
        [Display(Name = "Steam平台Id")]
        public string SteamId { get; set; }
        [Display(Name = "发行平台")]
        public string PlatformName { get; set; }
        [Display(Name = "版本名称")]
        [Required(ErrorMessage = "请输入版本名称")]
        public string EditionName { get; set; }
        [Display(Name = "版本类别")]
        public EditionType EditionType { get; set; }
        [Display(Name = "发行时间")]
        [Required(ErrorMessage = "请选择发行时间")]
        public DateTime IssueTime { get; set; }
        [Display(Name = "发行商")]
        public string Publisher { get; set; }
        [Display(Name = "引擎")]
        public string Engine { get; set; }
        [Display(Name = "介绍")]
        [StringLength(100000)]
        public string MainPage { get; set; }
    }
    public enum EditionType
    {
        [Display(Name = "正式版")]
        OfficialEdition,
        [Display(Name = "DEMO")]
        DEMO,
        [Display(Name = "DLC")]
        DLC,
        [Display(Name = "其他")]
        Other
    }
    public enum EntryType
    {
        [Display(Name = "游戏")]
        Game,
        [Display(Name = "角色")]
        Role,
        [Display(Name = "制作组")]
        ProductionGroup,
        [Display(Name = "STAFF")]
        Staff
    }

    public class BasicEntryInformation
    {
        public long Id { get; set; }
        public string Modifier { get; set; }

        public string DisplayName { get; set; }


        public string DisplayValue { get; set; }

        public ICollection<BasicEntryInformationAdditional> Additional { get; set; }
    }

    public class BasicEntryInformationAdditional
    {
        public long Id { get; set; }

        public string DisplayName { get; set; }


        public string DisplayValue { get; set; }
    }

    public class EntryRelevance
    {
        public long Id { get; set; }

        public string Modifier { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
        /// <summary>
        /// 当 类别 不是可识别时 使用下方链接
        /// </summary>
        public string Link { get; set; }
    }

    public class EntryPicture
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public enum GamePlatformType
    {
        Windows, Linux, Mac, IOS, Android, PS, NS, DOS, HarmonyOS
    }

    public enum PositionGeneralType
    {
        [Display(Name = "其他")]
        Other,
        [Display(Name = "视频")]
        Video,
        [Display(Name = "设计")]
        Design,
        [Display(Name = "音乐")]
        Music,
        [Display(Name = "作词")]
        ComposingWords,
        [Display(Name = "演唱")]
        Singing,
        [Display(Name = "剧本")]
        Script,
        [Display(Name = "配音")]
        CV,
        [Display(Name = "演出")]
        Show,
        [Display(Name = "美术")]
        FineArts,
        [Display(Name = "程序")]
        Program,
        [Display(Name = "运营")]
        Operate,
        [Display(Name = "发行")]
        Issue,
        [Display(Name = "制作")]
        Make,
        [Display(Name = "PV")]
        PV,
        [Display(Name = "后期")]
        LaterStage,
        [Display(Name = "主催")]
        MainUrge,
        [Display(Name = "策划")]
        Plan
    }

    public enum GenderType
    {
        [Display(Name = "保密")]
        None,
        [Display(Name = "男")]
        Man,
        [Display(Name = "女")]
        Women,
        [Display(Name = "其他")]
        Other
    }
}
