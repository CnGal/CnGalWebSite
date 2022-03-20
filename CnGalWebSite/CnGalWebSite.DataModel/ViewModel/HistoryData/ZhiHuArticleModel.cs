﻿using System;
namespace CnGalWebSite.DataModel.ViewModel.HistoryData
{
    public class ZhiHuArticleModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string OriginalAuthor { get; set; }

        public string Url { get; set; }

        public string MainPage { get; set; }

        public string Image { get; set; }

        public bool IsCutImage { get; set; }

        public DateTime PublishTime { get; set; }

        public DateTime? PostTime { get; set; }
    }
}
