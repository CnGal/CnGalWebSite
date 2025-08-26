using CnGalWebSite.Core.Services;
using CnGalWebSite.Extensions;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Services.SensitiveWords;
using CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService;
using CnGalWebSite.Kanban.ChatGPT.Services.UserAnalysisService;
using CnGalWebSite.Kanban.ChatGPT.Services.SelfAnalysisService;
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
        private readonly ISensitiveWordService _sensitiveWordService;
        private readonly IFunctionCallingService _functionCallingService;
        private readonly ISelfAnalysisService _selfAnalysisService;
        private readonly IUserAnalysisService _userAnalysisService;

        private static List<DateTime> _record = new List<DateTime>();
        private readonly HttpClient _httpClient;
        private string? _lastToolCallHash;
        private int _recursionCount;
        private readonly int MaxRecursionDepth;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public ChatGPTService(IHttpService httpService, IConfiguration configuration, ILogger<ChatGPTService> logger,
            IMemoryCache memoryCache, ISensitiveWordService sensitiveWordService, IFunctionCallingService functionCallingService, ISelfAnalysisService selfAnalysisService, IUserAnalysisService userAnalysisService)
        {
            _httpService = httpService;
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
            _sensitiveWordService = sensitiveWordService;
            _functionCallingService = functionCallingService;
            _selfAnalysisService = selfAnalysisService;
            _userAnalysisService = userAnalysisService;

            _httpClient = _httpService.GetClientAsync().GetAwaiter().GetResult();
            if (!int.TryParse(_configuration["MaxRecursionDepth"], out MaxRecursionDepth))
            {
                MaxRecursionDepth = 10;
            }
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

        private string GetToolCallHash(List<ToolCall> toolCalls)
        {
            if (toolCalls == null || !toolCalls.Any())
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var call in toolCalls)
            {
                sb.Append(call.Function.Name)
                  .Append(':')
                  .Append(call.Function.Arguments)
                  .Append(';');
            }
            return sb.ToString().GetSha1();
        }

        public async Task<ChatGPTSendMessageResult> SendMessages(List<ChatCompletionMessage> messages)
        {
            // 检查递归深度
            if (_recursionCount >= MaxRecursionDepth)
            {
                _logger.LogWarning("检测到可能的无限递归，已达到最大递归深度 {depth}", MaxRecursionDepth);

                _recursionCount = 0;

                return new ChatGPTSendMessageResult
                {
                    Success = true,
                    Message = "看板娘陷入了思考的循环中..."
                };
            }

            _recursionCount++;

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

            string? reply = null;


            // 检查敏感词 如果属于递归调用，不对内部输入检查敏感词
            var words = new List<string>();
            if (_recursionCount <= 1)
            {
                words = await _sensitiveWordService.Check(messages.Last().Content!);
            }

            if (words.Count != 0)
            {
                _logger.LogError("提问中检查到 {re} 个敏感词：\n      {}", words.Count, string.Join("\n      ", words));

                // 默认回复
                reply = "看板娘不知道哦~";
            }
            else
            {
                //日志
                _logger.LogInformation("向ChatGPT发送消息：{question}\n", messages.Last().Content!.Length >= 30 ? ($"{messages.Last().Content![..30].Replace("\n", "\n      ")}......") : messages.Last().Content!);
                //添加记录
                _record.Add(DateTime.Now);

                // api
                var url = _configuration["ChatGPTApiUrl"];

                var model = new ChatCompletionModel
                {
                    Model = string.IsNullOrWhiteSpace(_configuration["ChatGPTModel"]) ? "gpt-3.5-turbo" : _configuration["ChatGPTModel"]!,
                    Messages = messages
                };


                if (_configuration["EnableFunctionCalling"]?.ToLower() == "true")
                {
                    // 添加可用的工具
                    var tools = _functionCallingService.GetAvailableTools();
                    if (tools.Any())
                    {
                        model.Tools = tools;
                    }
                }

                var response = await _httpClient.PostAsJsonAsync(url + "v1/chat/completions", model);

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

                if (result == null)
                {
                    return new ChatGPTSendMessageResult
                    {
                        Success = false,
                        Message = "回复为空"
                    };
                }

                reply = result.Choices?.FirstOrDefault()?.Message?.Content;

                // 调用完直接输出用量统计
                _logger.LogInformation("收到ChatGPT的回复：{reply}\n      消耗 {} Token，回复占比 {}%（{}），命中缓存 {}%（{}）\n", reply, result.Usage.Total_tokens, (result.Usage.Completion_tokens * 100.0 / result.Usage.Total_tokens).ToString("0.0"), result.Usage.Completion_tokens, (result.Usage.prompt_cache_hit_tokens * 100.0 / result.Usage.Prompt_tokens).ToString("0.0"), result.Usage.prompt_cache_hit_tokens);


                // 处理函数调用
                var toolCalls = result.Choices?.FirstOrDefault()?.Message?.tool_calls;

                bool toolFail = false;

                if (toolCalls != null && toolCalls.Count != 0)
                {
                    // 检查是否与上一次工具调用相同
                    var currentHash = GetToolCallHash(toolCalls);
                    if (currentHash == _lastToolCallHash)
                    {
                        _logger.LogWarning("检测到重复的工具调用，终止递归");

                        _lastToolCallHash = null;

                        return new ChatGPTSendMessageResult
                        {
                            Success = true,
                            Message = "看板娘陷入了重复的操作中..."
                        };
                    }
                    _lastToolCallHash = currentHash;

                    foreach (var toolCall in toolCalls)
                    {
                        try
                        {
                            var functionResult = await _functionCallingService.ExecuteFunction(
                                toolCall.Function.Name,
                                toolCall.Function.Arguments
                            );

                            // 将函数调用结果添加到消息列表
                            messages.Add(new ChatCompletionMessage
                            {
                                Role = "assistant",
                                Content = null,
                                tool_calls = new List<ToolCall> { toolCall }
                            });

                            messages.Add(new ChatCompletionMessage
                            {
                                Role = "tool",
                                Content = functionResult,
                                tool_call_id = toolCall.Id
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "执行函数 {functionName} 失败", toolCall.Function.Name);
                            toolFail = true;
                            break;
                        }
                    }

                    if (toolFail == false)
                    {
                        // 继续对话
                        return await SendMessages(messages);
                    }
                }
                else
                {
                    // 重置递归计数和工具调用哈希
                    _recursionCount = 0;
                    _lastToolCallHash = null;
                }

                // 调用工具失败
                if (toolFail)
                {
                    return new ChatGPTSendMessageResult
                    {
                        Success = false,
                        Message = "呜呜呜~~~"
                    };
                }

                if (string.IsNullOrWhiteSpace(reply))
                {
                    return new ChatGPTSendMessageResult
                    {
                        Success = false,
                        Message = "回复为空"
                    };
                }

                // 分析回复内容并记录看板娘的自我信息
                _ = Task.Run(async () => await _selfAnalysisService.AnalyzeAndRecordSelfInfoAsync(reply));

                // 分析用户消息并记录用户信息
                _ = Task.Run(async () => await _userAnalysisService.AnalyzeAndRecordUserInfoAsync(messages));

                // 检查敏感词
                //words = await _sensitiveWordService.Check(reply);
                //if (words.Count != 0)
                //{
                //    _logger.LogError("回复中检查到 {re} 个敏感词：\n      {}", words.Count, string.Join("\n      ", words));

                //    // 默认回复
                //    reply = "看板娘不知道哦~";
                //}
            }

            // 缓存回复
            if (!string.IsNullOrWhiteSpace(reply))
            {
                SetCache(messages, reply);
            }

            return new ChatGPTSendMessageResult
            {
                Success = true,
                Message = reply
            };
        }
    }
}
