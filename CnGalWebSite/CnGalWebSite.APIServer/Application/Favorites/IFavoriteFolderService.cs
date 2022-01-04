using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteFolderService
    {
        Task<QueryData<ListFavoriteFolderAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListFavoriteFolderAloneModel searchModel, string userId = "");
    }
}
