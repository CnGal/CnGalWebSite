using BlazorComponent;
using CnGalWebSite.DataModel.ViewModel.Articles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.ViewModel.Articles
{
    public class NewsSummaryCacheModel
    {
        public StringNumber TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 10;

        public int TotalPages => (Items.Count / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;

        public List<NewsSummaryAloneViewModel> Items = new List<NewsSummaryAloneViewModel>();

    }
}
