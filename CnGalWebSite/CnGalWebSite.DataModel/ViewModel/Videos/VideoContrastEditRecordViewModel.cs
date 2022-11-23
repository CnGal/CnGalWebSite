using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Videos
{
    public class VideoContrastEditRecordViewModel
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }

        public VideoViewModel ContrastModel { get; set; }
        public VideoViewModel CurrentModel { get; set; }
    }
}
