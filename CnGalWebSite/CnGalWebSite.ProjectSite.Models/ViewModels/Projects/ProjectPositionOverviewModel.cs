using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{
    public class ProjectPositionOverviewModel
    {
        public long Id { get; set; }

        public long ProjectId { get; set; }

        public string ProjectName { get; set; }

        /// <summary>
        /// 职位详情
        /// </summary>
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
        /// 截稿日期
        /// </summary>
        public DateTime DeadLine { get; set; }

        /// <summary>
        /// 职位类型 小类
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tags { get; set; }

    }
}
