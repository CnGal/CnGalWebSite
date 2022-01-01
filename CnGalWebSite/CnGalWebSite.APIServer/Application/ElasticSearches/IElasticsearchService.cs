using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Home;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.ElasticSearches
{
    public interface IElasticsearchService
    {
        Task UpdateDataToElasticsearch(DateTime LastUpdateTime);

        Task DeleteDataOfElasticsearch();

        Task<PagedResultDto<SearchAloneModel>> QueryAsync(int page, int limit, string text, string screeningConditions, string sort, QueryType type);
    }
}
