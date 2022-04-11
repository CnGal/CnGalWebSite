using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.ViewModel.Robots;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Robots
{
    public interface IRobotService
    {
        Task<QueryData<ListRobotReplyAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListRobotReplyAloneModel searchModel);

        Task<QueryData<ListRobotGroupAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListRobotGroupAloneModel searchModel);

        Task<QueryData<ListRobotEventAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListRobotEventAloneModel searchModel);
    }
}
