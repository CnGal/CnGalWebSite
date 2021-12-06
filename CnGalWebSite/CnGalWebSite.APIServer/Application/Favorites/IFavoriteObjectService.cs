using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteObjectService
    {
        Task<QueryData<ListFavoriteObjectAloneModel>> GetPaginatedResult(QueryPageOptions options, ListFavoriteObjectAloneModel searchModel, long favoriteFolderId = 0);

        Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input, long favoriteFolderId);
    }
}
