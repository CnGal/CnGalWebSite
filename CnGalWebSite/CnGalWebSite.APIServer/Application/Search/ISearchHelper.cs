
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Search
{
    public interface ISearchHelper
    {
        Task UpdateDataToSearchService(DateTime LastUpdateTime, bool updateAll = false);

        Task DeleteDataOfSearchService();

        Task<PagedResultDto<SearchAloneModel>> QueryAsync(SearchInputModel model);

    }


    public enum QueryType
    {
        /// <summary>
        /// 分页
        /// </summary>
        Page,
        /// <summary>
        /// 无限下滑
        /// </summary>
        Index
    }
}
