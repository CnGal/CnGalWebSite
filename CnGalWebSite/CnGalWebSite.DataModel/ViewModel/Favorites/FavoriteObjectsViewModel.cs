using CnGalWebSite.DataModel.ViewModel.Search;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class FavoriteObjectAloneViewModel
    {
        public EntryInforTipViewModel entry { get; set; }

        public ArticleInforTipViewModel article { get; set; }

        public PeripheryInforTipViewModel periphery { get; set; }
    }
}
