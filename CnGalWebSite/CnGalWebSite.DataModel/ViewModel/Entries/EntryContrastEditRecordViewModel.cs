using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EntryContrastEditRecordViewModel
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }

        public EntryIndexViewModel ContrastModel { get; set; }
        public EntryIndexViewModel CurrentModel { get; set; }
    }
}
