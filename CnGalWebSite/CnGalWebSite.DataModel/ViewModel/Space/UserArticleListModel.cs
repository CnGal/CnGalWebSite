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

        public int TotalPages => ((Items.Count - 1) / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;

        public List<ArticleInforTipViewModel> Items { get; set; } = new List<ArticleInforTipViewModel>();
    }
}
