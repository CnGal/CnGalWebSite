using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Users.Dtos;

using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Messages
{
    public interface IMessageService
    {
        Task<PagedResultDto<DataModel.Model.Message>> GetPaginatedResult(GetMessageInput input, string userId);

        Task<QueryData<ListMessageAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListMessageAloneModel searchModel);
    }
}
