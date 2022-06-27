using CnGalWebSite.DataModel.Model;
using System;
namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class ArticleInforTipViewModel
    {
        public long Id { get; set; }

        public ArticleType Type { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string CreateUserName { get; set; }

        public string CreateUserId { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime LastEditTime { get; set; }

        public int ReaderCount { get; set; }

        public int ThumbsUpCount { get; set; }

        public int CommentCount { get; set; }

        public string Link { get; set; }
    }
}
