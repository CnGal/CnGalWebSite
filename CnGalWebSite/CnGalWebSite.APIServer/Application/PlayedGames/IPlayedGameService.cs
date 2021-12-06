using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.PlayedGames
{
    public interface IPlayedGameService
    {
        Task<PagedResultDto<EntryInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);
    }
}
