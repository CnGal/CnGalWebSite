using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Perfections
{

    public class ListPerfectionsViewModel
    {
        public List<ListPerfectionAloneModel> Perfections { get; set; } = new List<ListPerfectionAloneModel> { };
    }
    public class ListPerfectionAloneModel
    {
        [Display(Name = "词条Id")]
        public int Id { get; set; }
        [Display(Name = "唯一名称")]
        public string Name { get; set; }
        [Display(Name = "完善度")]
        public double Grade { get; set; }
        [Display(Name = "击败全站词条百分比")]
        public double VictoryPercentage { get; set; }

    }

    public class PerfectionsPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListPerfectionAloneModel SearchModel { get; set; }
    }
}
