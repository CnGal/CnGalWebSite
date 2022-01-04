using CnGalWebSite.DataModel.Model;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class MoveFavoriteObjectsModel
    {
        public long CurrentFolderId { get; set; }

        public long[] FolderIds { get; set; }

        public List<KeyValuePair<FavoriteObjectType, long>> ObjectIds { get; set; }
    }
}
