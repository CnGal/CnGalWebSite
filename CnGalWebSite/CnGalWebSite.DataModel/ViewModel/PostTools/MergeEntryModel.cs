using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PostTools
{
    public class MergeEntryModel
    {
        public int HostId { get; set; }
        public string HostName { get; set; }

        public int SubId { get; set; }
        public string SubName { get; set; }

        public List<BaseEditModel> Examines = new List<BaseEditModel>();

        public DateTime? PostTime { get; set; }

        public int TotalTaskCount { get; set; } = 10;
        public int CompleteTaskCount { get; set; }

        public string Error { get; set; }
    }
}
