using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class VideoInforTipViewModel
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string CreateUserName { get; set; }

        public string CreateUserId { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime LastEditTime { get; set; }

        public DateTime PubishTime { get; set; }

        public int ReaderCount { get; set; }

        public int CommentCount { get; set; }
    }
}
