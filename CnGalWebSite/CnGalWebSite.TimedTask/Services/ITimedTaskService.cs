using CnGalWebSite.TimedTask.Models.DataModels;
using System.Threading.Tasks;

namespace CnGalWebSite.TimedTask.Services
{
    public interface ITimedTaskService
    {
        /// <summary>
        /// 运行定时任务
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task RunTimedTask(TimedTaskModel item);
    }
}
