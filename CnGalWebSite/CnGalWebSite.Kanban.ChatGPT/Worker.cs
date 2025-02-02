
using CnGalWebSite.EventBus.Services;
using CnGalWebSite.Kanban.ChatGPT.Services.KanbanService;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.Kanban.ChatGPT
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
            var _kanbanService = provider.GetRequiredService<IKanbanService>();


            var server = new Random().Next(0, 2);


            _logger.LogInformation("服务端上线");
            _eventBusService.CreateKanbanServer(async (input) =>
            {
                //_logger.LogInformation("收到客户端消息：{input}", input.Message);

                var result = await _kanbanService.GetReply(input.Message, input.UserId, input.IsFirst, input.MessageMax);

                if (result.Success == false)
                {
                    _logger.LogError("发送回复：{re}", result.Message);
                }


                return new EventBus.Models.KanbanChatGPTReceiveModel
                {
                    Success = result.Success,
                    Message = result.Message
                };

            });

            _logger.LogInformation("客户端上线");
            _eventBusService.InitRpcClient();


            //// 1
            //var message = "你是傻逼";
            //// _logger.LogInformation("客户端发送消息：{message}", message);

            //var re = await _eventBusService.CallKanbanChatGPT(new EventBus.Models.KanbanChatGPTSendModel
            //{
            //    Message = message,
            //    IsFirst = true,
            //    UserId = "123",
            //    MessageMax = 3
            //}, stoppingToken);


            //if (re.Success == false)
            //{
            //    _logger.LogError("接收失败：{re}", re.Message);
            //}



            //// 2
            //message = "你是傻逼";

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
