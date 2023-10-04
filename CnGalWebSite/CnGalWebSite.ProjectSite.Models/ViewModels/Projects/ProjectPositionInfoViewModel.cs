using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{
    public class ProjectPositionInfoViewModel
    {
        public long Id { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 职位类型 大类
        /// </summary>
        public ProjectPositionType PositionType { get; set; }

        /// <summary>
        /// 职位类型名称
        /// </summary>
        public string PositionTypeName { get; set; }

        /// <summary>
        /// 职位类型 小类
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

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
        /// 紧急程度
        /// </summary>
        public PositionUrgencyType UrgencyType { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tags { get; set; }


        /// <summary>
        /// 创建用户
        /// </summary>
        public UserInfoViewModel UserInfo { get; set; } = new UserInfoViewModel();
    }
}
