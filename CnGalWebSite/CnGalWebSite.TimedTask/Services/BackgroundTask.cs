using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using CnGalWebSite.TimedTask.DataReositories;
using CnGalWebSite.TimedTask.Models.DataModels;
using CnGalWebSite.TimedTask.Services;


namespace CnGalWebSite.TimedTask.Services
{
    public class BackgroundTask : BackgroundService
    {

        public IServiceProvider Services { get; }

        public BackgroundTask(IServiceProvider services)
        {
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = Services.CreateScope();
            var _timedTaskRepository = scope.ServiceProvider.GetRequiredService<IRepository<TimedTaskModel, int>>();
            var _timedTaskService = scope.ServiceProvider.GetRequiredService<ITimedTaskService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var taskList = _timedTaskRepository.GetAllList(s => s.IsRuning == false && s.IsPause == false);

                    foreach (var item in taskList)
                    {
                        switch (item.ExecuteType)
                        {
                            case TimedTaskExecuteType.IntervalTime:
                                if (item.LastExecutedTime != null && item.LastExecutedTime.Value.AddMinutes(item.IntervalTime) >= DateTime.UtcNow)
                                {
                                    continue;
                                }
                                break;
                            case TimedTaskExecuteType.EveryDay:
                                if (item.LastExecutedTime != null && item.LastExecutedTime.Value.Date >= DateTime.UtcNow.Date)
                                {
                                    continue;
                                }
                                else
                                {
                                    if (item.EveryTime.Value.TimeOfDay >= DateTime.UtcNow.TimeOfDay)
                                    {
                                        continue;
                                    }
                                }
                                break;
                        }
                        await _timedTaskService.RunTimedTask(item);
                    }
                }
                catch
                {
                    //错误处理
                }

                //定时任务休眠
                await Task.Delay(60 * 1000, stoppingToken);
            }

        }

    }
}
