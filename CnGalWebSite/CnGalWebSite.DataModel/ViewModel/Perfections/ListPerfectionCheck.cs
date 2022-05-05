using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Perfections
{
    public class ListPerfectionChecksViewModel
    {
        public List<ListPerfectionCheckAloneModel> PerfectionChecks { get; set; } = new List<ListPerfectionCheckAloneModel> { };
    }
    public class ListPerfectionCheckAloneModel
    {
        [Display(Name = "词条Id")]
        public int Id { get; set; }
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "检查")]
        public PerfectionCheckType Type { get; set; }
        [Display(Name = "分数")]
        public double Grade { get; set; }
        [Display(Name = "超过其他词条该项百分比")]
        public double VictoryPercentage { get; set; }

    }

    public class PerfectionChecksPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListPerfectionCheckAloneModel SearchModel { get; set; }
    }
}
