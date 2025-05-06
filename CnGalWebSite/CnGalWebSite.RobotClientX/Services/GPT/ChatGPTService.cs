using CnGalWebSite.Core.Services;
using CnGalWebSite.EventBus.Models;
using CnGalWebSite.EventBus.Services;
using CnGalWebSite.RobotClientX.Models.GPT;
using CnGalWebSite.RobotClientX.Models.Messages;
using CnGalWebSite.RobotClientX.Services.ExternalDatas;
using CnGalWebSite.RobotClientX.Services.Messages;
using System.Text.Json;

namespace CnGalWebSite.RobotClientX.Services.GPT
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatGPTService> _logger;
        private readonly IGroupMessageCacheService _groupMessageCacheService;
        private readonly IEventBusService _eventBusService;

        public ChatGPTService(IHttpService httpService, IConfiguration configuration, ILogger<ChatGPTService> logger, IGroupMessageCacheService groupMessageCacheService, IEventBusService eventBusService)
        {
            _configuration = configuration;
            _logger = logger;
            _groupMessageCacheService = groupMessageCacheService;
            _eventBusService = eventBusService;
        }

        public async Task<string> GetReply(long sendTo)
        {
            var kanban = _configuration["QQ"];
            if (!long.TryParse(kanban, out long qq))
            {
                _logger.LogError("看板娘QQ不正确：{id}", kanban);
                return null;
            }
            var messages = _groupMessageCacheService.GetGroupMessages(sendTo);

            if (messages.Count == 0)
            {
                _logger.LogError("无法获取群聊历史记录：{id}", sendTo);
                return null;
            }

            // 判断是否需要清理历史消息
            var GroupHistoryMax = _configuration["GroupHistoryMax"];
            if (!int.TryParse(GroupHistoryMax, out int max))
            {
                max = 30;
            }
            var GroupHistoryMin = _configuration["GroupHistoryMin"];
            if (!int.TryParse(GroupHistoryMin, out int min))
            {
                min = 10;
            }

            if (_groupMessageCacheService.GetGroupMessages(sendTo).Count > max)
            {
                _groupMessageCacheService.KeepLatestMessages(sendTo, min);
            }

            // 拼接历史消息
            var model = new KanbanGroupGptModel
            {
                Messages = messages.Select(s => new KanbanGroupMessageModel
                {
                    IsAssistant = s.SenderId == qq,
                    Name = s.SenderName,
                    Text = s.Content
                }).ToList()
            };

            // 打印请求信息
            _logger.LogInformation("向看板娘发送请求：{model}", JsonSerializer.Serialize(model, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));

            var result = await _eventBusService.CallKanbanGroupChatGPT(model);
            if (result == null || result.Success == false)
            {
                _logger.LogError("获取GPT回复失败：{id}", sendTo);
                return null;
            }


            return result.Message.Replace($"【看板娘】\n", "").Replace("【看板娘】\\n", "").Replace("【看板娘】", "").TrimStart();
        }
    }
}
