using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteFolderService
    {
        Task<QueryData<ListFavoriteFolderAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFavoriteFolderAloneModel searchModel, string userId = "");
    }
}
