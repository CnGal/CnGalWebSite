using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PostTools
{
    public class MergeEntryModel:ToolTaskBase
    {
        public int HostId { get; set; }
        public string HostName { get; set; }

        public int SubId { get; set; }
        public string SubName { get; set; }

        public List<BaseEditModel> Examines = new List<BaseEditModel>();

        

        public MergeEntryModel()
        {
            TotalTaskCount = 10;
        }

    }
}
