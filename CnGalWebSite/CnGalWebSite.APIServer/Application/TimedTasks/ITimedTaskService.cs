
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.EventBus.Models;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.TimedTasks
{
    public interface ITimedTaskService
    {
        /// <summary>
        /// 运行定时任务
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        Task RunTimedTask(RunTimedTaskModel Model);
    }
}
