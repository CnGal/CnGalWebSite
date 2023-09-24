using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Perfections
{

    public class PerfectionCheckOverviewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 检查
        /// </summary>
        public PerfectionCheckType Type { get; set; }
        /// <summary>
        /// 分数
        /// </summary>
        public double Grade { get; set; }
        /// <summary>
        /// 超过其他词条该项百分比
        /// </summary>
        public double VictoryPercentage { get; set; }

    }

}
