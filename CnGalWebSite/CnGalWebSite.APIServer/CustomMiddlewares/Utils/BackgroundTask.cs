using CnGalWebSite.APIServer.Application.Tasks;
using CnGalWebSite.APIServer.Application.TimedTasks;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.EventBus.Models;
using CnGalWebSite.EventBus.Services;
using CnGalWebSite.TimedTask.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.BackgroundTasks
{
    public class BackgroundTask : BackgroundService
    {

        private readonly IServiceProvider _serviceProvider;
        private int _counter = 0;

        public BackgroundTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var _logger = scope.ServiceProvider.GetRequiredService<ILogger<BackgroundTask>>();
            var _applicationLifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
            var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var _eventBusService = scope.ServiceProvider.GetRequiredService<IEventBusService>();
            var _timedTaskService = scope.ServiceProvider.GetRequiredService<ITimedTaskService>();
            var _backgroundTaskService = scope.ServiceProvider.GetRequiredService<IBackgroundTaskService>();
            var _entryRepository = scope.ServiceProvider.GetRequiredService<IRepository<Entry, int>>();
            ConcurrentQueue<RunTimedTaskModel> _queue = new ConcurrentQueue<RunTimedTaskModel>();

            try
            {

                _logger.LogInformation("启动后台任务");

                if (string.IsNullOrWhiteSpace(_configuration["EventBus_HostName"]) == false)
                {
                    _eventBusService.RecieveRunTimedTask(_queue.Enqueue);
                }

                while (true)
                {
                    _backgroundTaskService.Runing();

                    if (!_queue.TryDequeue(out RunTimedTaskModel model))
                    {
                        await Task.Delay(100, stoppingToken);
                    }
                    else
                    {
                        await _timedTaskService.RunTimedTask(model);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "后台任务异常");
                //关闭
                //_applicationLifetime.StopApplication();
                //错误处理
                _backgroundTaskService.Fail();
            }
        }

    }
}
