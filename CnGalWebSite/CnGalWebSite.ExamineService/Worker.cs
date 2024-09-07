
using CnGalWebSite.EamineService.Services.SensitiveWords;
using CnGalWebSite.EventBus.Models;
using CnGalWebSite.EventBus.Services;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.EamineService
{

    public class Worker : BackgroundService
    {

        public IServiceProvider Services { get; }

        public Worker(IServiceProvider services, IHostApplicationLifetime applicationLifetime)
        {
            Services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            using var scope = Services.CreateScope();
            var provider = scope.ServiceProvider;
            var _logger = provider.GetRequiredService<ILogger<Worker>>();
            var _eventBusService = provider.GetRequiredService<IEventBusService>();
            var _sensitiveWordService = provider.GetRequiredService<ISensitiveWordService>();


            var server = new Random().Next(0, 2);


            _logger.LogInformation("服务端上线");
            _eventBusService.CreateSensitiveWordsCheckServer((input) =>
            {
                return Task.FromResult(new SensitiveWordsResultModel
                {
                    Words = _sensitiveWordService.Check(input.Texts)
                });
            });

            _logger.LogInformation("客户端上线");
            _eventBusService.InitRpcClient();

            var re = await _eventBusService.CallSensitiveWordsCheck(new SensitiveWordsCheckModel
            {
                Texts = ["傻逼煞笔沙比","12122112"] 
            }, stoppingToken);

            _logger.LogError("检查到 {re} 个敏感词：\n      {}", re.Words.Count, string.Join("\n      ", re.Words));




            //// 2
            //message = "看板娘知道《硅心》这个游戏吗？可以介绍一下吗？";

            //re = await _eventBusService.CallKanbanChatGPT(new EventBus.Models.KanbanChatGPTSendModel
            //{
            //    Message = message,
            //    IsFirst = false,
            //    UserId = "123",
            //    MessageMax = 3
            //}, stoppingToken);

            //_logger.LogInformation("接收消息{re}", re.Success ? "成功" : "失败");

            //// 3
            //message = "不对哦~《硅心》是由呐呐呐制作组制作的galgame。讲述了突然到来的机器人少女，打破了某自由插画师的隐世单机生活，一场人机恋爱喜剧由此开的故事。";

            //re = await _eventBusService.CallKanbanChatGPT(new EventBus.Models.KanbanChatGPTSendModel
            //{
            //    Message = message,
            //    IsFirst = false,
            //    UserId = "123",
            //    MessageMax = 3
            //}, stoppingToken);

            //_logger.LogInformation("接收消息{re}", re.Success ? "成功" : "失败");

            //// 4
            //message = "看板娘现在知道《硅心》是什么游戏了吗？";

            //re = await _eventBusService.CallKanbanChatGPT(new EventBus.Models.KanbanChatGPTSendModel
            //{
            //    Message = message,
            //    IsFirst = false,
            //    UserId = "123",
            //    MessageMax = 3
            //}, stoppingToken);

            //_logger.LogInformation("接收消息{re}", re.Success ? "成功" : "失败");


            //// 5
            //message = "看板娘真聪明";

            //re = await _eventBusService.CallKanbanChatGPT(new EventBus.Models.KanbanChatGPTSendModel
            //{
            //    Message = message,
            //    IsFirst = false,
            //    UserId = "123",
            //    MessageMax = 3
            //}, stoppingToken);

            //_logger.LogInformation("接收消息{re}", re.Success ? "成功" : "失败");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }
    }
}
