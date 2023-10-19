using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class Project: BaseModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 企划名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 企划详情
        /// </summary>
        [StringLength(10000000)]
        public string Description { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        [Obsolete("直接使用创建者的联系方式")]
        public string Contact { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 预览图
        /// </summary>
        public List<ProjectImage> Images { get; set; }

        /// <summary>
        /// 需求职位
        /// </summary>
        public List<ProjectPosition> Positions { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }

    }

    public class ProjectImage
    {
        public long Id { get;set; }

        public string Note { get; set; }

        public string Image { get; set; }

        public int Priority { get; set; }
    }

    public class ProjectPosition : BaseModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 职位详情
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 预算备注
        /// </summary>
        public string BudgetNote { get; set; }

        /// <summary>
        /// 预算类型
        /// </summary>
        public BudgetType BudgetType { get; set; }

        /// <summary>
        /// 预算区间上限
        /// </summary>
        public int BudgetMax { get; set; }

        /// <summary>
        /// 预算区间下限
        /// </summary>
        public int BudgetMin { get; set; }

        /// <summary>
        /// 分成比例
        /// </summary>
        public int Percentage { get; set; }

        /// <summary>
        /// 职位类型 大类
        /// </summary>
        public ProjectPositionType PositionType { get; set; }
        /// <summary>
        /// 职位类型名称
        /// </summary>
        public string PositionTypeName { get; set; }

        /// <summary>
        /// 截稿日期
        /// </summary>
        public DateTime DeadLine { get; set; }

        /// <summary>
        /// 紧急程度
        /// </summary>
        public PositionUrgencyType UrgencyType { get; set; }

        /// <summary>
        /// 职位类型 小类
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 应征用户
        /// </summary>
        public List<ProjectPositionUser> Users { get; set; }

        public long ProjectId { get; set; }
        public Project Project { get; set; }
    }

    public class ProjectPositionUser
    {
        public long Id { get; set; }

        public long PositionId { get; set; }
        public ProjectPosition Position { get; set; }

        public ApplicationUser User { get; set; }
        public string UserId { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        public bool? Passed { get; set; }
    }

    public enum PositionUrgencyType
    {
        [Display(Name ="无")]
        None,
        [Display(Name = "随缘")]
        Low,
        [Display(Name = "紧急")]
        High
    }

    public enum BudgetType
    {
        [Display(Name = "销售分成")]
        Divide,
        [Display(Name = "一次性报酬+销售分成")]
        IntervalAndDivide,
        [Display(Name = "一次性报酬")]
        Interval,
        [Display(Name = "用爱发电")]
        Afadian,
    }

    public enum ProjectPositionType
    {
        [Display(Name ="其他")]
        Other,
        [Display(Name = "配音")]
        CV,
        [Display(Name = "画师")]
        Painter,
        [Display(Name = "程序")]
        Programmer,
        [Display(Name = "剧本")]
        Writer,
        [Display(Name = "音乐")]
        Music
    }
}
