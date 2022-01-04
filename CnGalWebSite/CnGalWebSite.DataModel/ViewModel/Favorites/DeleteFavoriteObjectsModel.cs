using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class DeleteFavoriteObjectsModel
    {
        public long FavorieFolderId { get; set; }

        public long[] Ids { get; set; }
    }
}
