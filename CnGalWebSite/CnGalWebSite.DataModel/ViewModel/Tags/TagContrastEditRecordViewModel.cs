using CnGalWebSite.DataModel.ViewModel.Peripheries;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class TagContrastEditRecordViewModel
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }

        public TagIndexViewModel ContrastModel { get; set; }
        public TagIndexViewModel CurrentModel { get; set; }
    }
}
