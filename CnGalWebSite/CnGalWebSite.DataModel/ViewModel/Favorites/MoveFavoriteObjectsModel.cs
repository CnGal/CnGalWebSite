using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class MoveFavoriteObjectsModel
    {
        public long CurrentFolderId { get; set; }

        public long[] FolderIds { get; set; }

        public List<KeyValuePair<FavoriteObjectType, long>> ObjectIds { get; set; }
    }
}
