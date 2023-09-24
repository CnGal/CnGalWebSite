
using CnGalWebSite.DataModel.ViewModel.Favorites;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Favorites
{
    public interface IFavoriteObjectService
    {
        Task<PagedResultDto<FavoriteObjectAloneViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input, long favoriteFolderId);
    }
}
