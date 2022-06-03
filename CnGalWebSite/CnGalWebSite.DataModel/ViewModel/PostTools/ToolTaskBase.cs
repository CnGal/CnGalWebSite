using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PostTools
{
    public class ToolTaskBase
    {

        public int TotalTaskCount { get; set; } 
        public int CompleteTaskCount { get; set; }

        public string Error { get; set; }
    }
}
