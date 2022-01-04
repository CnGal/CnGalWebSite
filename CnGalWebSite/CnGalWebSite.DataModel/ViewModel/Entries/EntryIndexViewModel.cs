using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
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
        /// 是否查看编辑记录状态
        /// </summary>
        public bool IsExamineList { get; set; }

        public int DisambigId { get; set; }

        public string DisambigName { get; set; }

        [Display(Name = "制作组")]
        public string ProductionGroup { get; set; }
        [Display(Name = "发行商")]
        public string Publisher { get; set; }

        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState ImagesState { get; set; }
        public EditState RelevancesState { get; set; }
        public EditState InforState { get; set; }
        public EditState TagState { get; set; }

        public bool CanComment { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public EntryType Type { get; set; }

        /// <summary>
        /// 附加信息列表
        /// </summary>
        public List<InformationsModel> Information { get; set; }
        /// <summary>
        /// staff信息 
        /// </summary>
        public List<StaffInforModel> Staffs { get; set; }

        /// <summary>
        /// 动态动态
        /// </summary>
        public List<NewsModel> NewsOfEntry { get; set; }

        /// <summary>
        /// 角色信息 
        /// </summary>
        public List<RoleInforModel> Roles { get; set; }

        /// <summary>
        /// STAFF的关联游戏列表
        /// </summary>
        public List<StaffGameModel> StaffGames { get; set; }

        /// <summary>
        /// 制作组的关联游戏列表
        /// </summary>
        public List<EntryInforTipViewModel> GroupGames { get; set; }

        /// <summary>
        /// 主页
        /// </summary>
        public string MainPage { get; set; }

        /// <summary>
        /// 相关词条
        /// </summary>
        public List<EntryInforTipViewModel> EntryRelevances { get; set; }

        /// <summary>
        /// 相关文章
        /// </summary>
        public List<ArticleInforTipViewModel> ArticleRelevances { get; set; }

        /// <summary>
        /// 外部链接
        /// </summary>
        public List<RelevancesKeyValueModel> OtherRelevances { get; set; }


        /// <summary>
        /// 图片列表
        /// </summary>
        public List<PicturesViewModel> Pictures { get; set; }
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> Examines { get; set; }

        /// <summary>
        /// 标签列表
        /// </summary>
        public List<TagsViewModel> Tags { get; set; }
    }
    public class TagsViewModel
    {
        public string Name { get; set; }
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
        locked

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

        public DateTime HappenedTime { get; set; }
    }

    public class StaffGameModel
    {
        public string Name { get; set; }
        public string BriefIntroduction { get; set; }
        public string Link { get; set; }

        public List<string> Positions { get; set; }

        public string Image { get; set; }
    }

    public class StaffInforModel
    {
        public string Modifier { get; set; }
        public List<StaffValue> StaffList { get; set; }
    }

    public class StaffValue
    {
        public string Modifier { get; set; }
        public List<StaffNameModel> Names { get; set; }
    }

    public class StaffNameModel
    {
        public string DisplayName { get; set; }

        public string RealName { get; set; }
    }

    public class RoleInforModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public string CV { get; set; }

        public string ImagePath { get; set; }

        public string BriefIntroduction { get; set; }
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
