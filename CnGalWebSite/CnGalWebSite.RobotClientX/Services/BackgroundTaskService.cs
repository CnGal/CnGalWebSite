using CnGalWebSite.EventBus.Services;
using CnGalWebSite.RobotClientX.Services.QQClients;

namespace CnGalWebSite.RobotClientX.Services
{
    public class BackgroundTaskService : BackgroundService
    {

        public IServiceProvider Services { get; }
        private readonly IHostApplicationLifetime _applicationLifetime;

        public BackgroundTaskService(IServiceProvider services, IHostApplicationLifetime applicationLifetime)
        {
            Services = services;
            _applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await new TaskFactory().StartNew(async () =>
            {
                using var scope = Services.CreateScope();
                var provider = scope.ServiceProvider;
                var _logger = provider.GetRequiredService<ILogger<BackgroundTaskService>>();
                var _eventBusService = provider.GetRequiredService<IEventBusService>();
                try
                {
                    _logger.LogInformation("启动后台任务");

                    var _QQClientService = provider.GetRequiredService<IQQClientService>();
                    await _QQClientService.Init();

                    // 初始化RPC客户端
                    _eventBusService.InitRpcClient();

                    while (true)
                        await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "后台任务异常");
                    //关闭
                    _applicationLifetime.StopApplication();
                    //错误处理
                }
            }, stoppingToken);
        }
    }
}
