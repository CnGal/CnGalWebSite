using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class UserArticleListModel
    {
        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 10;

        public int TotalCount { get; set; }

        public int TotalPages => MaxCount <= 0 ? 0 : (int)Math.Ceiling(decimal.Divide(TotalCount, MaxCount));

        public int CurrentPage { get; set; } = 1;

        public List<ArticleInforTipViewModel> Items { get; set; } = new List<ArticleInforTipViewModel>();
    }
}
