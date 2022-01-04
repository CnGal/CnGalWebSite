using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class EditArticlePriorityViewModel
    {
        public long[] Ids { get; set; } = Array.Empty<long>();

        public int PlusPriority { get; set; }
    }
}
