using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EstablishEntryViewModel
    {

        #region 主要信息
        [Display(Name = "唯一名称")]
        [Required(ErrorMessage = "请填写唯一名称")]
        public string Name { get; set; }
        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        public string DisplayName { get; set; }
        [Display(Name = "别称")]
        public string AnotherName { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }
        //[Required(ErrorMessage = "请上传缩略图")]
        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }
        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }
        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }

        [Display(Name = "类别")]
        [Required(ErrorMessage = "请选择类别")]
        public EntryType Type { get; set; }
        #endregion

        #region 附加信息

        #region 游戏
        [Display(Name = "发行时间")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? IssueTime { get; set; }
        [Display(Name = "发行时间备注")]
        public string IssueTimeString { get; set; }
        [Display(Name = "原作")]
        public string Original { get; set; }
        [Display(Name = "制作组")]
        public string ProductionGroup { get; set; }
        [Display(Name = "游戏平台")]
        public List<GamePlatformModel> GamePlatforms { get; set; } = new List<GamePlatformModel>() { };
        [Display(Name = "引擎")]
        public string Engine { get; set; }
        [Display(Name = "发行商")]
        public string Publisher { get; set; }
        [Display(Name = "发行方式")]
        public string IssueMethod { get; set; }
        [Display(Name = "官网")]
        public string OfficialWebsite { get; set; }
        [Display(Name = "Steam平台Id")]
        public string SteamId { get; set; }
        [Display(Name = "QQ群")]
        public string QQgroupGame { get; set; }
        [Display(Name = "STAFF")]
        public List<StaffModel> InforStaffs { get; set; } = new List<StaffModel>() { };

        #endregion
        #region 角色

        [Display(Name = "声优")]
        public string CV { get; set; }
        [Display(Name = "性别")]
        public GenderType Gender { get; set; }
        [Display(Name = "身材数据")]
        public string FigureData { get; set; }
        [Display(Name = "身材(主观)")]
        public string FigureSubjective { get; set; }
        [Display(Name = "生日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }
        [Display(Name = "发色")]
        public string Haircolor { get; set; }
        [Display(Name = "瞳色")]
        public string Pupilcolor { get; set; }
        [Display(Name = "服饰")]
        public string ClothesAccessories { get; set; }
        [Display(Name = "性格")]
        public string Character { get; set; }
        [Display(Name = "角色身份")]
        public string RoleIdentity { get; set; }
        [Display(Name = "血型")]
        public string BloodType { get; set; }
        [Display(Name = "身高")]
        public string RoleHeight { get; set; }
        [Display(Name = "兴趣")]
        public string RoleTaste { get; set; }
        [Display(Name = "年龄")]
        public string RoleAge { get; set; }

        #endregion
        #region Staff
        [Display(Name = "微博平台Id")]
        public string WeiboId { get; set; }
        [Display(Name = "Bilibili平台Id")]
        public string BilibiliId { get; set; }
        [Display(Name = "AcFun平台Id")]
        public string AcFunId { get; set; }
        [Display(Name = "知乎平台Id")]
        public string ZhihuId { get; set; }
        [Display(Name = "爱发电平台Id（不包括@）")]
        public string AfdianId { get; set; }

        [Display(Name = "Pixiv平台Id")]
        public string PixivId { get; set; }
        [Display(Name = "Twitter平台Id")]
        public string TwitterId { get; set; }
        [Display(Name = "YouTube平台Id")]
        public string YouTubeId { get; set; }
        [Display(Name = "Facebook平台Id")]
        public string FacebookId { get; set; }

        #endregion
        #region 制作组
        [Display(Name = "QQ群")]
        public string QQgroupGroup { get; set; }
        #endregion

        [Display(Name = "相关网站")]
        public List<SocialPlatform> SocialPlatforms { get; set; } = new List<SocialPlatform>();
        #endregion

        #region 主页
        public string Context { get; set; } = "";
        #endregion

        #region 图片
        public List<EditImageAloneModel> Images { get; set; } = new List<EditImageAloneModel>() { };

        #endregion

        #region 关联词条
        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> ReStaffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> News { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> articles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Others { get; set; } = new List<RelevancesModel>();
        #endregion

        #region 标签
        public List<RelevancesModel> Tags { get; set; } = new List<RelevancesModel>();
        #endregion

        [Display(Name = "备注")]
        public string Note { get; set; }

    }
}
