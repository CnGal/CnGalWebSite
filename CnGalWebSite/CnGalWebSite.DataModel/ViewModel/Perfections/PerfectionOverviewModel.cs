using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Perfections
{
    public class PerfectionOverviewModel
    {
        /// <summary>
        /// 词条Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 完善度
        /// </summary>
        public double Grade { get; set; }
        /// <summary>
        /// 击败全站词条百分比
        /// </summary>
        public double VictoryPercentage { get; set; }

    }
}
