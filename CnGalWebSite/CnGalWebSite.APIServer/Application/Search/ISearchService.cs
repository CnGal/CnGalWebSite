
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.ViewModel.Home;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search
{
    public interface ISearchService
    {
        Task<PagedResultDto<SearchAloneModel>> GetPaginatedResult(GetSearchInput input);
    }
}
