using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{
    public class ProjectBaseModel
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
        public List<ProjectImageViewModel> Images { get; set; } = new List<ProjectImageViewModel>();


        /// <summary>
        /// 需求职位
        /// </summary>
        public List<ProjectPositionViewModel> Positions { get; set; } = new List<ProjectPositionViewModel>();
    }

    public class ProjectViewModel
    {
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

    }

    public class ProjectImageViewModel : BaseImageEditModel
    {
        public long Id { get; set; }
    }

    public class ProjectPositionViewModel
    {
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
    }
}
