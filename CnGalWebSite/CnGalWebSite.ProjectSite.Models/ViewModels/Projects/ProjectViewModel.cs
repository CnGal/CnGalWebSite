using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{ 
    public class ProjectViewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 企划名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 企划详情
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 预览图
        /// </summary>
        public List<ProjectImageViewModel> Images { get; set; } = new List<ProjectImageViewModel>();

        /// <summary>
        /// 需求职位
        /// </summary>
        public List<ProjectPositionViewModel> Positions { get; set; } = new List<ProjectPositionViewModel>();
        /// <summary>
        /// 创建者
        /// </summary>
        public UserInfoViewModel CreateUser { get; set; } = new UserInfoViewModel();

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        public int TabIndex { get; set; }
    }

    public class ProjectImageViewModel : BaseImageEditModel
    {

    }

    public class ProjectPositionViewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 职位详情
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 预算区间备注
        /// </summary>
        public string BudgetNote { get; set; }

        /// <summary>
        /// 职位类型 大类型
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
        /// 职位类型 小类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 紧急程度
        /// </summary>
        public PositionUrgencyType UrgencyType { get; set; }

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
        /// 应征用户
        /// </summary>
        public List<ProjectPositionUserViewModel> Users { get; set; } = new List<ProjectPositionUserViewModel>();

    }

    public class ProjectPositionUserViewModel
    {
        public long Id { get; set; }

        public string Contact { get; set; }

        public bool? Passed { get; set; }

        public bool showReply { get; set; }

        public bool showComment { get; set; } = true;

        /// <summary>
        /// 应征用户
        /// </summary>
        public UserInfoViewModel User { get; set; } = new UserInfoViewModel();


    }
}
