using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.EditRecords
{
    public class ListUserMonitorsViewModel
    {
        public List<ListUserMonitorAloneModel> UserMonitorEntries { get; set; } = new List<ListUserMonitorAloneModel> { };
    }
    public class ListUserMonitorAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "词条Id")]
        public int EntryId { get; set; }
        [Display(Name = "类型")]
        public EntryType Type { get; set; }
        [Display(Name = "名称")]
        public string EntryName { get; set; }
        [Display(Name = "添加监视的时间")]
        public DateTime CreateTime { get; set; }
    }

    public class UserMonitorsPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListUserMonitorAloneModel SearchModel { get; set; }
    }
}
