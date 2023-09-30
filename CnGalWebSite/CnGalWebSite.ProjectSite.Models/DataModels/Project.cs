using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class Project
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
        public string Contact { get; set; }

        /// <summary>
        /// 预算区间
        /// </summary>
        public string BudgetRange { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 预览图
        /// </summary>
        public List<ProjectImage> Images { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

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

    public class ProjectPosition
    {
        public long Id { get; set; }

        /// <summary>
        /// 职位详情
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 预算区间
        /// </summary>
        public string BudgetRange { get; set; }

        /// <summary>
        /// 职位类型
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
        /// 职位类型
        /// </summary>
        public string Type { get; set; }

        public long ProjectId { get; set; }
        public Project Project { get; set; }
    }

    public enum ProjectPositionType
    {
        Other,
        CV,
        Painter,
        Programmer,
        Writer
    }
}
