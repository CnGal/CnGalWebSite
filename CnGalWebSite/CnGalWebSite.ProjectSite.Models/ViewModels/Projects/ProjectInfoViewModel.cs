using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{
    public class ProjectInfoViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

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
        public double Percentage { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
