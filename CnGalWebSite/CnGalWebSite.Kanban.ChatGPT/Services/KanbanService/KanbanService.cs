using CnGalWebSite.Core.Services;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Services.ChatGPTService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.KanbanService
{
    public class KanbanService(IMemoryCache memoryCache, ILogger<KanbanService> logger, IConfiguration configuration, IChatGPTService chatGPTService) : IKanbanService
    {

        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly IChatGPTService _chatGPTService = chatGPTService;
        private readonly ILogger<KanbanService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;


        public void SetCache(List<ChatCompletionMessage> messages, string userId)
        {
            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };

            _ = options.RegisterPostEvictionCallback(OnPostEviction);

            // 缓存用户对话记录
            _memoryCache.Set(userId, messages, options);
        }

        public void OnPostEviction(object key, object? obj, EvictionReason reason, object? state)
        {
            if (obj is List<ChatCompletionMessage> messageList)
            {
                _logger.LogInformation("用户对话记录缓存过期，用户Id：{userId}，对话次数：{reply}\n", key, messageList.Count(s => s.Role == "user"));
            }
        }


        public async Task<ChatGPTSendMessageResult> GetReply(string message, string userId, bool first, int messageMax)
        {
            var datetime = DateTime.Now.ToCstTime();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "用户Id为空"
                };
            }

            if (string.IsNullOrWhiteSpace(message) && first == false)
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "消息为空"
                };
            }

            // 检查消息长度
            if (message != null && message.Length > int.Parse(_configuration["ChatGPTLimit_Length"] ?? "30"))
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "消息长度超过上限"
                };
            }

            // 尝试从缓存中获取用户对话记录


            if (_memoryCache.TryGetValue(userId, out var value) && value is List<ChatCompletionMessage> messageList)
            {
                _logger.LogInformation("用户对话记录命中缓存，用户Id：{userId}，对话次数：{msg}\n", userId, messageList.Count(s => s.Role == "user"));
            }
            else
            {
                // 没有找到缓存对话记录 开始第一次对话
                first = true;
                messageList = new List<ChatCompletionMessage>();
            }


            //  检查上限
            if (messageList.Count(s => s.Role == "user") >= messageMax)
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "对话次数达到上限"
                };
            }

            bool cache = false;

            // 判断是否为第一次回复
            if (first)
            {
                // 判断是否有历史记录
                if (messageList.Count > 1)
                {
                    messageList.RemoveRange(2, messageList.Count - 2);
                    cache = true;
                    _logger.LogInformation("用户清空对话记录，但是返回缓存：{}\n", messageList[1].Content);
                }
                else
                {
                    // 是则拼接开场白
                    messageList.Clear();

                    var sys = _configuration["ChatGPT_SystemMessageTemplate"];

                    if (string.IsNullOrWhiteSpace(sys))
                    {
                        return new ChatGPTSendMessageResult
                        {
                            Success = false,
                            Message = "消息模板为空"
                        };
                    }

                    //填充消息模板
                    sys = sys.Replace("{time}", datetime.ToString("HH:mm:ss"));
                    sys = sys.Replace("{date}", datetime.ToString("yyyy年MM月dd日"));


                    messageList.Add(new ChatCompletionMessage
                    {
                        Role = "system",
                        Content = sys
                    });
                }


            }
            else
            {
                // 拼接历史对话

                messageList.Add(new ChatCompletionMessage
                {
                    Role = "user",
                    Content = message
                });
            }


            ChatGPTSendMessageResult? result = null;
            if (cache == false)
            {
                // 请求结果
                result = await _chatGPTService.SendMessages(messageList);
                if (result.Success == false)
                {
                    return new ChatGPTSendMessageResult
                    {
                        Success = false,
                        Message = result.Message
                    };
                }

                // 缓存对话
                messageList.Add(new ChatCompletionMessage
                {
                    Role = "assistant",
                    Content = result.Message,
                });
            }
            else
            {
                result = new ChatGPTSendMessageResult
                {
                    Success = true,
                    Message = messageList[1].Content
                };

            }

            SetCache(messageList, userId);

            return result;
        }

    }
}
