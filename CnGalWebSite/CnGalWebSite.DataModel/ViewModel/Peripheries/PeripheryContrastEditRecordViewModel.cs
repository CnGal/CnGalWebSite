using CnGalWebSite.DataModel.ViewModel.Articles;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class PeripheryContrastEditRecordViewModel
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }

        public PeripheryViewModel ContrastModel { get; set; }
        public PeripheryViewModel CurrentModel { get; set; }
    }
}
