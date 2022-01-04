using BootstrapBlazor.Components;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.TimedTasks;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.TimedTasks
{
    public interface ITimedTaskService
    {
        Task<QueryData<ListTimedTaskAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListTimedTaskAloneModel searchModel);

        /// <summary>
        /// 运行定时任务
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task RunTimedTask(TimedTask item);
    }
}
