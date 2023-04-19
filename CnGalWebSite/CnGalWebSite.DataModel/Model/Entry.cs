using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }

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
        public bool? CanComment { get; set; } = true;
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// 是否隐藏外部链接
        /// </summary>
        public bool IsHideOutlink { get; set; }
        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }
        /// <summary>
        /// 游玩数
        /// </summary>
        public int PlayedCount { get; set; }
        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 游戏发布时间 只对游戏词条有效
        /// </summary>
        public DateTime? PubulishTime { get; set; }
        /// <summary>
        /// 使用的模板
        /// </summary>
        public EntryStyleTemplate Template { get; set; }
        /// <summary>
        /// 消歧义页
        /// </summary>
        public int? DisambigId { get; set; }
        public Disambig Disambig { get; set; }
        /// <summary>
        /// 备份管理
        /// </summary>
        public long? BackUpArchiveId { get; set; }
        public BackUpArchive BackUpArchive { get; set; }
        /// <summary>
        /// 完善度检查
        /// </summary>
        public int? PerfectionId { get; set; }
        public Perfection Perfection { get; set; }

        /// <summary>
        /// 官网补充信息
        /// </summary>
        public EntryWebsite WebsiteAddInfor { get; set; }

        /// <summary>
        /// 预约信息 只对游戏生效
        /// </summary>
        public Booking Booking { get; set; }

        /// <summary>
        /// 附加信息列表 Staff迁移到 EntryStaffFromEntryNavigation 中
        /// </summary>
        public ICollection<BasicEntryInformation> Information { get; set; } = new List<BasicEntryInformation>();

        /// <summary>
        /// 主页
        /// </summary>
        [StringLength(1000000)]
        public string MainPage { get; set; }

        /// <summary>
        /// 相关性列表
        /// </summary>
        public ICollection<EntryRelevance> Relevances { get; set; } = new List<EntryRelevance>();
        /// <summary>
        /// 周边列表
        /// </summary>
        [Obsolete("关联周边已迁移，请访问RelatedPeripheries")]
        public ICollection<PeripheryRelevanceEntry> Peripheries { get; set; } = new List<PeripheryRelevanceEntry>();

        /// <summary>
        /// 图片列表
        /// </summary>
        public ICollection<EntryPicture> Pictures { get; set; } = new List<EntryPicture>();
        /// <summary>
        /// 图片列表
        /// </summary>
        public ICollection<EntryAudio> Audio { get; set; } = new List<EntryAudio>();
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; } = new List<Examine>();
        /// <summary>
        /// 标签
        /// </summary>
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        /// <summary>
        /// 关联文章
        /// </summary>
        public ICollection<Article> Articles { get; set; } = new List<Article>();
        /// <summary>
        /// 关联文章
        /// </summary>
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        /// <summary>
        /// 关联周边
        /// </summary>
        public virtual ICollection<Periphery> RelatedPeripheries { get; set; } = new List<Periphery>();
        /// <summary>
        /// 关联外部链接
        /// </summary>
        public ICollection<Outlink> Outlinks { get; set; } = new List<Outlink>();
        /// <summary>
        /// 发行列表
        /// </summary>
        public virtual ICollection<GameRelease> Releases { get; set; } = new List<GameRelease>();

        /// <summary>
        /// 评分列表 也是已玩用户列表
        /// </summary>
        public ICollection<PlayedGame> PlayedGames { get; set; } = new HashSet<PlayedGame>();

        /// <summary>
        /// 投票
        /// </summary>
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

        /// <summary>
        /// 用户认证
        /// </summary>
        public UserCertification Certification { get; set; }

        /// <summary>
        /// 请求监视的用户
        /// </summary>
        public virtual ICollection<UserMonitor> Monitors { get; set; }


        /// <summary>
        /// 关联词条列表
        /// </summary>
        public virtual ICollection<EntryRelation> EntryRelationFromEntryNavigation { get; set; } = new List<EntryRelation>();

        /// <summary>
        /// To 指当前词条被关联的其他词条关联 呈现编辑视图时不使用
        /// </summary>
        public virtual ICollection<EntryRelation> EntryRelationToEntryNavigation { get; set; } = new List<EntryRelation>();

        /// <summary>
        /// 关联Staff列表
        /// </summary>
        public virtual ICollection<EntryStaff> EntryStaffFromEntryNavigation { get; set; } = new List<EntryStaff>();

        /// <summary>
        /// To 指当前词条被关联的其他Staff关联 用于快速查找反向关联
        /// </summary>
        public virtual ICollection<EntryStaff> EntryStaffToEntryNavigation { get; set; } = new List<EntryStaff>();

    }

    /// <summary>
    /// 词条官方网站补充信息
    /// </summary>
    public class EntryWebsite
    {
        public long Id { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public virtual ICollection<EntryWebsiteImage> Images { get; set; } = new List<EntryWebsiteImage>();

        /// <summary>
        /// 介绍 纯html
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 自定义Html 优先级最高
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// 标题 纯html
        /// </summary>
        public string FirstPage { get; set; }

        /// <summary>
        /// Logo图片
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        /// 副标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 主题语句
        /// </summary>
        public string Impressions { get; set; }

        /// <summary>
        /// 主题颜色
        /// </summary>
        public string Color { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }
    }

    public enum EntryStyleTemplate
    {
        [Display(Name = "缺省")]
        None,
        [Display(Name = "大卡片")]
        Default,
        [Display(Name = "文章介绍")]
        ArticleIntroduce,
        [Display(Name ="专题页")]
        OfficialWebsite
      
    }

    public class EntryWebsiteImage
    {
        public long Id { get; set; }

        public EntryWebsiteImageType Type { get; set; }

        public EntryWebsiteImageSize Size { get; set; }

        public string Url { get; set; }

        public int Priority { get; set; }

        public string Note { get; set; }
    }

    public enum EntryWebsiteImageSize
    {
        [Display(Name ="PC端")]
        Large,
        [Display(Name = "移动端")]
        Small
    }

    public enum EntryWebsiteImageType
    {
        [Display(Name = "轮播图")]
        Carousel,
        [Display(Name = "背景图")]
        Background
    }

    public class Outlink
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public string Link { get; set; }
    }

    public partial class EntryRelation
    {
        public long EntryRelationId { get; set; }

        public int? FromEntry { get; set; }
        public int? ToEntry { get; set; }

        public virtual Entry FromEntryNavigation { get; set; } = new Entry();
        public virtual Entry ToEntryNavigation { get; set; } = new Entry();
    }

    public partial class EntryStaff
    {
        public long EntryStaffId { get; set; }

        public int? FromEntry { get; set; }
        public int? ToEntry { get; set; }

        /// <summary>
        /// 仅当关联Staff为空时展示
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 优先展示显示名称 与其他地方不一致的行为
        /// </summary>
        public string CustomName { get; set; }
        /// <summary>
        /// 官方职位
        /// </summary>
        public string PositionOfficial { get; set; }
        /// <summary>
        /// 通用职位
        /// </summary>
        public PositionGeneralType PositionGeneral { get; set; }
        /// <summary>
        /// 隶属组织
        /// </summary>
        public string SubordinateOrganization { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// 游戏
        /// </summary>
        public virtual Entry FromEntryNavigation { get; set; }
        /// <summary>
        /// Staff
        /// </summary>
        public virtual Entry ToEntryNavigation { get; set; }
    }


    public class GameRelease
    {
        public long Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 发行平台类型
        /// </summary>
        public PublishPlatformType PublishPlatformType { get; set; }

        /// <summary>
        /// 发行平台名称
        /// </summary>
        public string PublishPlatformName { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public GameReleaseType Type { get; set; }

        /// <summary>
        /// 游戏平台
        /// </summary>
        public GamePlatformType[] GamePlatformTypes { get; set; } = new GamePlatformType[0];

        /// <summary>
        /// 发行时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 发行时间备注
        /// </summary>
        public string TimeNote { get; set; }

        /// <summary>
        /// 引擎
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }
    }

    public enum PublishPlatformType
    {
        [Display(Name = "Steam")]
        Steam,
        [Display(Name = "TapTap")]
        TapTap,
        [Display(Name = "Epic")]
        Epic,
        [Display(Name = "App Store")]
        AppStore,
        [Display(Name = "Google Play")]
        GooglePlay,
        [Display(Name = "其他")]
        Other,
    }
    public enum GameReleaseType
    {
        [Display(Name = "正式版")]
        Official,
        [Display(Name = "Demo")]
        Demo,
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
        [Display(Name = "组织")]
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

        public ICollection<BasicEntryInformationAdditional> Additional { get; set; } = new List<BasicEntryInformationAdditional>();
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
        [Display(Name = "分类")]
        public string Modifier { get; set; }
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "链接")]
        public string Url { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
    }

    public class EntryAudio
    {
        public long Id { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "链接")]
        public string Url { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
        [Display(Name = "时长")]
        public TimeSpan Duration { get; set; }

        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }
    }

    public class RoleBirthday
    {
        public long Id { get; set; }

        public DateTime Birthday { get;set;}

        public Entry Role { get; set; }
        public int RoleId { get; set; }
    }

    public enum GamePlatformType
    {
        [Display(Name = "Windows")]
        Windows,
        [Display(Name = "Linux")]
        Linux,
        [Display(Name = "macOS")]
        Mac,
        [Display(Name = "iOS")]
        IOS,
        [Display(Name = "Android")]
        Android,
        [Display(Name = "PS")]
        PS,
        [Display(Name = "NS")]
        NS,
        [Display(Name = "DOS")]
        DOS,
        [Display(Name = "HarmonyOS")]
        HarmonyOS,
        [Display(Name = "H5")]
        H5
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
        Plan,
        [Display(Name = "作曲")]
        ComposeMusic,
        [Display(Name = "制作组")]
        ProductionGroup,
        [Display(Name = "发行商")]
        Publisher,
        [Display(Name = "特别感谢")]
        SpecialThanks
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
