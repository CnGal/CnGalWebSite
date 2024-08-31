using CnGalWebSite.Core.Services;
using CnGalWebSite.Extensions;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.ChatGPTService
{
    public class ChatGPTService : IChatGPTService
    {
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatGPTService> _logger;
        private readonly IMemoryCache _memoryCache;

        private static List<DateTime> _record = new List<DateTime>();
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ChatGPTService(IHttpService httpService, IConfiguration configuration, ILogger<ChatGPTService> logger, IMemoryCache memoryCache)
        {
            _httpService = httpService;
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;

            _httpClient = _httpService.GetClientAsync().GetAwaiter().GetResult();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _configuration["ChatGPTApiKey"]);
        }


        public string? TryGetCache(List<ChatCompletionMessage> messages)
        {
            var json = JsonSerializer.Serialize(messages);
            var sha1 = json.GetSha1();

            // 尝试从缓存中获取GPT回复
            if (_memoryCache.TryGetValue(sha1, out var value) && value is string reply)
            {
                _logger.LogInformation("GPT回复命中缓存，sha1：{userId}，回复：{reply}\n", sha1, reply);
                return reply;
            }

            return null;
        }


        public void SetCache(List<ChatCompletionMessage> messages, string reply)
        {
            var json = JsonSerializer.Serialize(messages);
            var sha1 = json.GetSha1();

            MemoryCacheEntryOptions options = new()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
            };

            _ = options.RegisterPostEvictionCallback(OnPostEviction);

            // 缓存GPT回复
            _memoryCache.Set(sha1, reply, options);
        }

        public void OnPostEviction(object key, object? obj, EvictionReason reason, object? state)
        {
            if (obj is string reply)
            {
                _logger.LogInformation("GPT回复缓存过期，sha1：{userId}，回复：{reply}\n", key, reply);
            }
        }

        public bool CheckLimit()
        {
            var datetime = DateTime.Now;
            //检查上限
            if (_record.Count(s => s > datetime.AddMinutes(-1)) > int.Parse(_configuration["ChatGPTLimit_1_Minute"] ?? "10"))
            {
                return false;
            }
            //检查上限
            if (_record.Count(s => s > datetime.AddDays(-1)) > int.Parse(_configuration["ChatGPTLimit_1_Day"] ?? "1000"))
            {
                return false;
            }
            //清理过期的记录
            _record.RemoveAll(s => s < datetime.AddDays(-1));

            return true;
        }



        public async Task<ChatGPTSendMessageResult> SendMessages(List<ChatCompletionMessage> messages)
        {
            // 判断空值
            if (messages.Count == 0 || string.IsNullOrWhiteSpace(messages.Last().Content))
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "输入消息列表或最新内容为空"
                };
            }

            // 检查缓存
            var cache = TryGetCache(messages);
            if (cache != null)
            {
                return new ChatGPTSendMessageResult
                {
                    Success = true,
                    Message = cache
                };
            }


            //检查上限
            if (CheckLimit() == false)
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "限流"
                };
            }



            //日志
            _logger.LogInformation("向ChatGPT发送消息：{question}\n", messages.Last().Content!.Length >= 30 ? ($"{messages.Last().Content![..30].Replace("\n", "\n      ")}......") : messages.Last().Content!);
            //添加记录
            _record.Add(DateTime.Now);


            // api
            var url = _configuration["ChatGPTApiUrl"];

            var response = await _httpClient.PostAsJsonAsync(url + "v1/chat/completions", new ChatCompletionModel
            {
                Model = string.IsNullOrWhiteSpace(_configuration["ChatGPTModel"]) ? "gpt-3.5-turbo" : _configuration["ChatGPTModel"]!,
                Messages = messages
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("请求ChatGPT回复失败，正文：{msg}", await response.Content.ReadAsStringAsync());
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "请求 GPT API 失败"
                };
            }

            string jsonContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChatResult>(jsonContent, _jsonOptions);

            var reply = result?.Choices?.FirstOrDefault()?.Message?.Content;
            if (result == null || string.IsNullOrWhiteSpace(reply))
            {
                return new ChatGPTSendMessageResult
                {
                    Success = false,
                    Message = "回复为空"
                };
            }

            _logger.LogInformation("收到ChatGPT的回复：{reply}\n      消耗 {} Token，回复占比 {}%（{}）\n", reply, result.Usage.Total_tokens, (result.Usage.Completion_tokens * 100.0 / result.Usage.Total_tokens).ToString("0.0"), result.Usage.Completion_tokens);

            // 缓存回复
            SetCache(messages, reply);

            return new ChatGPTSendMessageResult
            {
                Success = true,
                Message = reply
            };
        }
    }
}
