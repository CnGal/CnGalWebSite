using CnGalWebSite.DataModel.Model;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Perfections
{
    public class PerfectionCheckViewModel
    {
        public long Id { get; set; }

        public double Grade { get; set; }

        public PerfectionCheckLevel Level { get; set; }

        /// <summary>
        /// 全站同类型检查的百分比 
        /// </summary>
        public double VictoryPercentage { get; set; }

        /// <summary>
        /// Infor信息相同的检查个数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 额外信息 例如缺失关联的词条的名称
        /// </summary>
        public string Infor { get; set; }

        public string EntryName { get; set; }

        public int EntryId { get; set; }

        public PerfectionCheckType CheckType { get; set; }

        public PerfectionDefectType DefectType { get; set; }
    }

}
