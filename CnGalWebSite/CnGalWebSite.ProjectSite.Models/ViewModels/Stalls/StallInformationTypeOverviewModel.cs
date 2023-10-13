using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Stalls
{
    public class StallInformationTypeViewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
    }

    public class StallInformationTypeOverviewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 支持的类型
        /// </summary>
        public List<ProjectPositionType> Types { get; set; }=new List<ProjectPositionType>();

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 介绍
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hide { get; set; }

        public bool HideInfoCard { get; set; }

        public int Priority { get; set; }

    }

    public class StallInformationTypeEditModel: StallInformationTypeOverviewModel
    {

    }
}
