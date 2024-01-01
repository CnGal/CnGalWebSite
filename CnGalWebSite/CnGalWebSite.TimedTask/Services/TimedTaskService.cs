
using CnGalWebSite.TimedTask.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CnGalWebSite.TimedTask.DataReositories;
using CnGalWebSite.Extensions;
using CnGalWebSite.EventBus.Services;

namespace CnGalWebSite.TimedTask.Services
{
    public class TimedTaskService : ITimedTaskService
    {
        private readonly ILogger<TimedTaskService> _logger;
        private readonly IRepository<TimedTaskModel, int> _timedTaskRepository;
        private readonly IEventBusService _eventBusService;

        public TimedTaskService(ILogger<TimedTaskService> logger, IRepository<TimedTaskModel, int> timedTaskRepository, IEventBusService eventBusService)
        {
            _logger = logger;
            _timedTaskRepository = timedTaskRepository;
            _eventBusService = eventBusService;
        }

        public async Task RunTimedTask(TimedTaskModel item)
        {
            if (item.IsRuning == true || item.IsPause == true)
            {
                return;
            }
            try
            {
                //更新执行状态
                _timedTaskRepository.Clear();
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsRuning = true;
                    await _timedTaskRepository.UpdateAsync(item);
                }
                //根据不同类型任务进行调用
                //向用户推送消息
                _eventBusService.SendRunTimedTask(new EventBus.Models.RunTimedTaskModel
                {
                    Note = item.Name,
                    Parameter = item.Parameter,
                    Type = (int)item.Type,
                    LastExecutedTime = item.LastExecutedTime,
                });
                //记录执行时间
                _timedTaskRepository.Clear();
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsLastFail = false;
                    item.IsRuning = false;
                    item.LastExecutedTime = DateTime.UtcNow;
                    await _timedTaskRepository.UpdateAsync(item);
                }
                _logger.LogInformation("成功发送定时任务消息：{name}", item.Name ?? item.Type.GetDisplayName());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送定时任务消息失败：{name}", item.Name ?? item.Type.GetDisplayName());

                _timedTaskRepository.Clear();
                item = await _timedTaskRepository.FirstOrDefaultAsync(s => s.Id == item.Id);
                if (item != null)
                {
                    item.IsLastFail = false;
                    item.IsRuning = false;
                    item.IsLastFail = true;
                    item.LastExecutedTime = DateTime.UtcNow;
                    await _timedTaskRepository.UpdateAsync(item);
                }
            }
            finally
            {

            }
        }

    }
}
