using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EntryIndexViewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
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
        /// Steam平台Id
        /// </summary>
        public int SteamId { get; set; }
        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEdit { get; set; }
        /// <summary>
        /// 是否处于隐藏状态
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// 是否隐藏外部链接
        /// </summary>
        public bool IsHideOutlink { get; set; }
        /// <summary>
        /// 是否有评分 仅限游戏词条
        /// </summary>
        public bool IsScored { get; set; }
        /// <summary>
        /// 是否配音 仅限游戏词条
        /// </summary>
        public bool IsDubbing { get; set; } = true;

        /// <summary>
        /// 制作组
        /// </summary>
        public List<StaffNameModel> ProductionGroups { get; set; } = new List<StaffNameModel>();
        /// <summary>
        /// 发行商
        /// </summary>
        public List<StaffNameModel> Publishers { get; set; } = new List<StaffNameModel>();

        /// <summary>
        /// 主要信息编辑状态
        /// </summary>
        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState ImagesState { get; set; }
        public EditState RelevancesState { get; set; }
        public EditState InforState { get; set; }
        public EditState TagState { get; set; }
        public EditState AudioState { get; set; }

        public bool CanComment { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public EntryType Type { get; set; }

        /// <summary>
        /// 预约
        /// </summary>
        public BookingViewModel Booking { get; set; }

        /// <summary>
        /// 附加信息列表
        /// </summary>
        public List<InformationsModel> Information { get; set; } = new List<InformationsModel>();
        /// <summary>
        /// Staff信息 
        /// </summary>
        public List<StaffInforModel> Staffs { get; set; } = new List<StaffInforModel> { };

        /// <summary>
        /// 动态
        /// </summary>
        public List<NewsModel> NewsOfEntry { get; set; } = new List<NewsModel> { };

        /// <summary>
        /// 游戏的关联角色列表  和相关词条没有重叠
        /// </summary>
        public List<EntryInforTipViewModel> Roles { get; set; } = new List<EntryInforTipViewModel> { };

        /// <summary>
        /// STAFF的关联游戏列表 和相关词条没有重叠
        /// </summary>
        public List<EntryInforTipViewModel> StaffGames { get; set; } = new List<EntryInforTipViewModel> { };

        /// <summary>
        /// 主页
        /// </summary>
        public string MainPage { get; set; }

        /// <summary>
        /// 相关词条
        /// </summary>
        public List<EntryInforTipViewModel> EntryRelevances { get; set; } = new List<EntryInforTipViewModel>();

        /// <summary>
        /// 相关文章
        /// </summary>
        public List<ArticleInforTipViewModel> ArticleRelevances { get; set; } = new List<ArticleInforTipViewModel> { };


        /// <summary>
        /// 相关视频
        /// </summary>
        public List<VideoInforTipViewModel> VideoRelevances { get; set; } = new List<VideoInforTipViewModel> { };

        /// <summary>
        /// 外部链接
        /// </summary>
        public List<RelevancesKeyValueModel> OtherRelevances { get; set; } = new List<RelevancesKeyValueModel> { };


        /// <summary>
        /// 图片列表
        /// </summary>
        public List<PicturesViewModel> Pictures { get; set; } = new List<PicturesViewModel>();
        /// <summary>
        /// 音频列表
        /// </summary>
        public List<AudioViewModel> Audio { get; set; } = new List<AudioViewModel>();

        /// <summary>
        /// 标签列表
        /// </summary>
        public List<TagsViewModel> Tags { get; set; } = new List<TagsViewModel> { };
    }

    public class BookingViewModel
    {
        public long BookingCount { get; set; }

        public List<BookingGoalViewModel> Goals=new List<BookingGoalViewModel>();
    }

    public class BookingGoalViewModel
    {
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 当前目标达成的最低人数
        /// </summary>
        public int Target { get; set; }
    }

    public class TagsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AudioViewModel
    {
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
    public class PicturesViewModel
    {
        public string Modifier { get; set; }

        public List<PicturesAloneViewModel> Pictures { get; set; }
    }

    public class PicturesAloneViewModel
    {
        [Display(Name = "备注")]
        public string Note { get; set; }
        [Display(Name = "链接")]
        public string Url { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
    }

    public enum EditState
    {
        [Display(Name = "待填写")]
        None,
        [Display(Name = "可编辑")]
        Normal,
        [Display(Name = "预览")]
        Preview,
        [Display(Name = "锁定")]
        Locked

    }
    public class NewsModel
    {
        public string Title { get; set; }
        public string BriefIntroduction { get; set; }
        public string Link { get; set; }

        public string GroupName { get; set; }
        public int GroupId { get; set; }
        public string NewsType { get; set; }
        public string Image { get; set; }

        /// <summary>
        /// GroupId无效则使用作者Id
        /// </summary>
        public string UserId { get; set; }

        public DateTime HappenedTime { get; set; }

        /// <summary>
        /// 文章Id
        /// </summary>
        public long ArticleId { get; set; }
    }

    public class StaffInforModel
    {
        /// <summary>
        /// 分组
        /// </summary>
        public string Modifier { get; set; }
        public List<StaffValue> StaffList { get; set; }
    }

    public class StaffValue
    {
        /// <summary>
        /// 职位
        /// </summary>
        public string Modifier { get; set; }
        public List<StaffNameModel> Names { get; set; }
    }

    public class StaffNameModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string DisplayName { get; set; }

        public int Id { get; set; }
    }

    public class InformationsModel
    {
        public string Modifier { get; set; }
        public List<KeyValueModel> Informations { get; set; } = new List<KeyValueModel>();
    }
    public class KeyValueModel
    {
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
    }

    public class RelevancesViewModel
    {
        public string Modifier { get; set; }
        public List<RelevancesKeyValueModel> Informations { get; set; } = new List<RelevancesKeyValueModel>();
    }
    public class RelevancesKeyValueModel
    {
        public string Image { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
        public string Link { get; set; }

        public long Id { get; set; }
    }
}
