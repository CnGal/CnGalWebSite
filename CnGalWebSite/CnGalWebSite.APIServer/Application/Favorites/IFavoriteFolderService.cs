using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ExamineModel.FavoriteFolders;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteFolderService
    {
      void  UpdateMain(FavoriteFolder favoriteFolder, FavoriteFolderMain examine);

       void UpdateData(FavoriteFolder favoriteFolder, Examine examine);

        FavoriteFolderViewModel GetViewModel(FavoriteFolder folder);
    }
}
