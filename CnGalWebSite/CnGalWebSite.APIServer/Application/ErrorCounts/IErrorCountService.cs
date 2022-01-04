using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.ErrorCounts
{
    public interface IErrorCountService
    {
        Task<QueryData<ListErrorCountAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListErrorCountAloneModel searchModel);
    }
}
