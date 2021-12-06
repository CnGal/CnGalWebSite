using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class AddFavoriteObjectViewModel
    {
        public long[] FavoriteFolderIds { get; set; }

        public FavoriteObjectType Type { get; set; }

        public long ObjectId { get; set; }
    }
}
