using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Votes
{
    public interface IVoteService
    {
        Task<QueryData<ListVoteAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListVoteAloneModel searchModel);
    }
}
