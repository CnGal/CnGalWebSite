using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class IsObjectInUserFavoriteFolderModel
    {
        public FavoriteObjectType Type { get; set; }

        public long ObjectId { get; set; }
    }

    public class IsObjectInUserFavoriteFolderResult
    {
        public bool Result { get; set; }
    }
}
