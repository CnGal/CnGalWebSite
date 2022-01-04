using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class Perfection
    {
        public long Id { get; set; }

        public double Grade { get; set; }

        public double VictoryPercentage { get; set; }

        public int EntryId { get; set; }
        public Entry Entry { get; set; }

        public ICollection<PerfectionCheck> Checks { get; set; }
    }

    public class PerfectionCheck
    {
        public long Id { get; set; }

        public double Grade { get; set; }

        public double VictoryPercentage { get; set; }

        /// <summary>
        /// Infor信息相同的检查个数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 额外信息 例如缺失关联的词条的名称
        /// </summary>
        public string Infor { get; set; }

        public PerfectionCheckType CheckType { get; set; }

        public PerfectionDefectType DefectType { get; set; }

        public long? PerfectionId { get; set; }
        public Perfection Perfection { get; set; }
    }

    public enum PerfectionCheckLevel
    {
        [Display(Name = "高")]
        High,
        [Display(Name = "中")]
        Middle,
        [Display(Name = "低")]
        Low,
        [Display(Name = "已完成")]
        None,
    }


    public enum PerfectionLevel
    {
        [Display(Name = "待完善")]
        ToBeImproved,
        [Display(Name = "良好")]
        Good,
        [Display(Name = "优秀")]
        Excellent,
    }


    public enum PerfectionDefectType
    {
        [Display(Name = "无")]
        None,
        [Display(Name = "未填写")]
        NotFilledIn,
        [Display(Name = "长度不足")]
        InsufficientLength,
        /// <summary>
        /// 特指 关联的词条文章不存在
        /// </summary>
        [Display(Name = "不存在")]
        NonExistent,
        [Display(Name = "类型错误")]
        TypeError,
        [Display(Name = "反向关联不存在")]
        ReverseAssociationNonExistent
    }

    public enum PerfectionCheckType
    {
        //通用
        [Display(Name = "简介")]
        BriefIntroduction,
        [Display(Name = "主图")]
        MainImage,
        [Display(Name = "主页")]
        MainPage,
        [Display(Name = "相册")]
        Images,
        [Display(Name = "标签")]
        Tags,
        /// <summary>
        /// 不属于特定于某个类别词条的关联部分
        /// </summary>
        [Display(Name = "关联文章")]
        RelevanceArticles,
        [Display(Name = "关联词条")]
        RelevanceEntries,

        //游戏
        [Display(Name = "角色")]
        Roles,
        [Display(Name = "Staff")]
        Staff,
        [Display(Name = "Steam平台Id")]
        SteamId,
        [Display(Name = "制作组")]
        ProductionGroup,
        [Display(Name = "发行商")]
        Publisher,
        [Display(Name = "发行时间")]
        IssueTime,
        [Display(Name = "游戏平台")]
        GamePlatforms,
        [Display(Name = "官网")]
        OfficialWebsite,
        [Display(Name = "QQ群")]
        QQgroup,

    }

}
