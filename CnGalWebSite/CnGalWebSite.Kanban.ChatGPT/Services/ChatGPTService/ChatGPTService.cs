using CnGalWebSite.Core.Services;
using CnGalWebSite.Extensions;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Services.SensitiveWords;
using CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService;
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
        private readonly ISelfMemoryService _selfMemoryService;

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
            IMemoryCache memoryCache, ISensitiveWordService sensitiveWordService, IFunctionCallingService functionCallingService, ISelfMemoryService selfMemoryService)
        {
            _httpService = httpService;
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
            _sensitiveWordService = sensitiveWordService;
            _functionCallingService = functionCallingService;
            _selfMemoryService = selfMemoryService;

            _httpClient = _httpService.GetClientAsync().GetAwaiter().GetResult();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _configuration["ChatGPTApiKey"]);
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
                _ = Task.Run(async () => await AnalyzeAndRecordSelfInfoAsync(reply));

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

        /// <summary>
        /// 使用AI分析看板娘的回复并自动记录相关信息
        /// </summary>
        /// <param name="reply">看板娘的回复内容</param>
        private async Task AnalyzeAndRecordSelfInfoAsync(string reply)
        {
            try
            {
                // 提高分析阈值，避免频繁分析短消息
                if (string.IsNullOrWhiteSpace(reply))
                    return;

                // 获取现有记忆，让AI基于此进行去重分析
                var existingMemory = await _selfMemoryService.GetFormattedMemoryAsync("all");
                var analysisResult = await AnalyzeReplyWithAIAsync(reply, existingMemory);

                if (analysisResult != null)
                {
                    // 直接记录AI去重后的结果
                    await ProcessAnalysisResultDirectAsync(analysisResult);
                }

                _logger.LogInformation("AI自动分析看板娘回复完成，内容长度：{length}", reply.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI自动分析看板娘回复失败：{error}", ex.Message);
            }
        }

        /// <summary>
        /// 使用AI分析回复内容并自动去重
        /// </summary>
        /// <param name="reply">看板娘的回复</param>
        /// <param name="existingMemory">现有记忆（格式化后的字符串）</param>
        /// <returns>分析结果</returns>
        private async Task<SelfAnalysisResult?> AnalyzeReplyWithAIAsync(string reply, string existingMemory)
        {
            try
            {
                var analysisPrompt = $@"你需要分析看板娘的最新回复，并与她的现有记忆对比，只提取**全新且重要**的信息。

【看板娘现有记忆】：
{existingMemory}

【看板娘最新回复】：
{reply}

请分析最新回复，**只提取不在现有记忆中的新信息**，以JSON格式返回：

{{
  ""states"": [
    {{
      ""content"": ""具体的状态描述"",
      ""type"": ""状态类型(physical/emotional/activity/mental)"",
      ""duration"": 预计持续时间(分钟，0表示瞬时状态)
    }}
  ],
  ""traits"": [
    {{
      ""content"": ""具体的特征或偏好描述"",
      ""type"": ""类型(preference/skill/limitation/personality)"",
      ""intensity"": 强度(1-5)
    }}
  ],
  ""commitments"": [
    {{
      ""content"": ""具体的承诺或计划描述"",
      ""type"": ""类型(intention/promise/plan)"",
      ""expectedTime"": ""预期时间(ISO格式，可为null)""
    }}
  ]
}}

**严格的去重和提取原则**：
1. **对比现有记忆**：仔细检查新信息是否已经在现有记忆中存在
2. **语义去重**：即使表达方式不同，但意思相同的信息不要重复记录
3. **聚焦真正的新变化**：只记录确实新出现的状态、特征、承诺
4. **忽略重复表达**：如果又提到草莓、西瓜、可爱语气等已记录的内容，不要再记录
5. **避免琐碎信息**：不要记录无意义的细节或套话
6. **如果没有任何新信息，所有数组都返回空**
7. 只返回JSON，不要其他解释文字";

                var analysisMessages = new List<ChatCompletionMessage>
                {
                    new() { Role = "system", Content = "你是一个专门分析文本内容的AI助手，专注于提取人物的状态、偏好和承诺信息。" },
                    new() { Role = "user", Content = analysisPrompt }
                };

                var analysisRequest = new ChatCompletionModel
                {
                    Model = string.IsNullOrWhiteSpace(_configuration["ChatGPTModel"]) ? "gpt-3.5-turbo" : _configuration["ChatGPTModel"]!,
                    Messages = analysisMessages,
                    temperature = 0.1
                };

                var url = _configuration["ChatGPTApiUrl"];
                var analysisResponse = await _httpClient.PostAsJsonAsync(url + "v1/chat/completions", analysisRequest);

                if (!analysisResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI分析请求失败：{status}", analysisResponse.StatusCode);
                    return null;
                }

                var analysisContent = await analysisResponse.Content.ReadAsStringAsync();
                var analysisResult = JsonSerializer.Deserialize<ChatResult>(analysisContent, _jsonOptions);

                var content = analysisResult?.Choices?.FirstOrDefault()?.Message?.Content;

                if (content == null)
                {
                    _logger.LogWarning("AI分析响应为空");
                    return null;
                }

                var jsonContent = content.Trim();

                // 移除可能的markdown代码块标记
                if (jsonContent.StartsWith("```json"))
                {
                    jsonContent = jsonContent.Substring(7);
                }
                if (jsonContent.EndsWith("```"))
                {
                    jsonContent = jsonContent.Substring(0, jsonContent.Length - 3);
                }

                var result = JsonSerializer.Deserialize<SelfAnalysisResult>(jsonContent, _jsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI分析回复内容失败");
                return null;
            }
        }

        /// <summary>
        /// 直接处理AI去重后的分析结果
        /// </summary>
        /// <param name="analysisResult">AI已去重的分析结果</param>
        private async Task ProcessAnalysisResultDirectAsync(SelfAnalysisResult analysisResult)
        {
            try
            {
                // 记录状态信息
                if (analysisResult.States != null)
                {
                    foreach (var state in analysisResult.States)
                    {
                        if (!string.IsNullOrWhiteSpace(state.Content))
                        {
                            await _selfMemoryService.RecordSelfStateAsync(state.Content, state.Type, state.Duration);
                            _logger.LogInformation("AI记录新状态：{content}，类型：{type}", state.Content, state.Type);
                        }
                    }
                }

                // 记录特征和偏好
                if (analysisResult.Traits != null)
                {
                    foreach (var trait in analysisResult.Traits)
                    {
                        if (!string.IsNullOrWhiteSpace(trait.Content))
                        {
                            await _selfMemoryService.RecordSelfTraitAsync(trait.Content, trait.Type, trait.Intensity);
                            _logger.LogInformation("AI记录新特征：{content}，类型：{type}", trait.Content, trait.Type);
                        }
                    }
                }

                // 记录承诺和计划
                if (analysisResult.Commitments != null)
                {
                    foreach (var commitment in analysisResult.Commitments)
                    {
                        if (!string.IsNullOrWhiteSpace(commitment.Content))
                        {
                            DateTime? expectedTime = null;
                            if (!string.IsNullOrWhiteSpace(commitment.ExpectedTime) &&
                                DateTime.TryParse(commitment.ExpectedTime, out var parsedTime))
                            {
                                expectedTime = parsedTime;
                            }

                            await _selfMemoryService.MakeCommitmentAsync(commitment.Content, commitment.Type, expectedTime);
                            _logger.LogInformation("AI记录新承诺：{content}，类型：{type}", commitment.Content, commitment.Type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理AI分析结果失败");
            }
        }

        /// <summary>
        /// AI自我分析结果模型
        /// </summary>
        private class SelfAnalysisResult
        {
            public List<SelfAnalysisState>? States { get; set; }
            public List<SelfAnalysisTrait>? Traits { get; set; }
            public List<SelfAnalysisCommitment>? Commitments { get; set; }
        }

        private class SelfAnalysisState
        {
            public string Content { get; set; } = "";
            public string Type { get; set; } = "";
            public int Duration { get; set; }
        }

        private class SelfAnalysisTrait
        {
            public string Content { get; set; } = "";
            public string Type { get; set; } = "";
            public int Intensity { get; set; }
        }

        private class SelfAnalysisCommitment
        {
            public string Content { get; set; } = "";
            public string Type { get; set; } = "";
            public string? ExpectedTime { get; set; }
        }
    }
}
