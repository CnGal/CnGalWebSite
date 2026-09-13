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
            var _backgroundTaskService = scope.ServiceProvider.GetRequiredService<IBackgroundTaskService>();
            ConcurrentQueue<RunTimedTaskModel> _queue = new ConcurrentQueue<RunTimedTaskModel>();

            try
            {

                _logger.LogInformation("启动后台任务");

                try
                {
                    var _eventBusService = scope.ServiceProvider.GetRequiredService<IEventBusService>();
                    // 定时任务
                    _eventBusService.RecieveRunTimedTask(_queue.Enqueue);

                    // RPC远程过程调用 客户端
                    _eventBusService.InitRpcClient();
                    _logger.LogInformation("Event bus enabled for background tasks");
                }
                catch (ConfigurationException ex)
                {
                    _logger.LogWarning(ex,
                        "Event bus disabled due to configuration error in section {Section}; timed-task consumer and RPC client were not initialized",
                        ex.Section);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    _backgroundTaskService.Runing();

                    if (!_queue.TryDequeue(out RunTimedTaskModel model))
                    {
                        await Task.Delay(100, stoppingToken);
                    }
                    else
                    {
                        try
                        {
                            using var taskScope = _serviceProvider.CreateScope();
                            var taskService = taskScope.ServiceProvider.GetRequiredService<ITimedTaskService>();
                            await taskService.RunTimedTask(model);
                        }
                        catch (ConfigurationException ex)
                        {
                            _logger.LogWarning("Skipping task due to configuration: {Section}", ex.Section);
                        }
                    }
                }

            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex) when (ex is not ConfigurationException)
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
