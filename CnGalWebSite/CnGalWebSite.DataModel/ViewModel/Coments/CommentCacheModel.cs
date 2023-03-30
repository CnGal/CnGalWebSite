using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Coments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Helper.ViewModel.Comments
{
    public class CommentCacheModel
    {
        public int TabIndex { get; set; } = 1;

        public int MaxCount { get; set; } = 8;

        public int TotalPages => ((Items.Count - 1) / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;

        public List<CommentViewModel> Items { get; set; } = new List<CommentViewModel>();
    }
}
