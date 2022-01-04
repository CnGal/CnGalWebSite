using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Files;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Files
{
    public interface IFileService
    {
        Task<PagedResultDto<ImageInforTipViewModel>> GetPaginatedResult(PagedSortedAndFilterInput input);

        Task<QueryData<ListFileAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListFileAloneModel searchModel);

        Task<string> SaveImageAsync(string url, string userId, double x = 0, double y = 0);

    }
}
