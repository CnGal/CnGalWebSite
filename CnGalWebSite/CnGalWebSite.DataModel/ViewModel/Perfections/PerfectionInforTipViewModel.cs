using CnGalWebSite.DataModel.Model;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Perfections
{
    public class PerfectionInforTipViewModel
    {
        public long Id { get; set; }

        public double Grade { get; set; }

        public double VictoryPercentage { get; set; }

        public DateTime LastEditTime { get; set; }

        public int EditCount { get; set; }

        public int DefectCount { get; set; }

        public string EntryName { get; set; }

        public int EntryId { get; set; }

        public PerfectionLevel Level { get; set; }
    }

}
