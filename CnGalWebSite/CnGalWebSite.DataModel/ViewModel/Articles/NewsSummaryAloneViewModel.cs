using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class NewsSummaryAloneViewModel
    {
        public string GroupName { get; set; }

        public string GroupImage { get; set; }

        public int GroupId { get; set; }

        public string UserId { get; set; }

        public string Outlink { get; set; }

        public string BriefIntroduction { get; set; }

        public List<ArticleInforTipViewModel> Articles { get; set; }
    }
}
