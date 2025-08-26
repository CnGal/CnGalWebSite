using CnGalWebSite.Core.Services;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Models.SelfAnalysis;
using CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.SelfAnalysisService
{
    /// <summary>
    /// 看板娘自我分析服务实现
    /// </summary>
    public class SelfAnalysisService : ISelfAnalysisService
    {
        private readonly ILogger<SelfAnalysisService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ISelfMemoryService _selfMemoryService;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public SelfAnalysisService(ILogger<SelfAnalysisService> logger, IConfiguration configuration,
            IHttpService httpService, ISelfMemoryService selfMemoryService)
        {
            _logger = logger;
            _configuration = configuration;
            _selfMemoryService = selfMemoryService;

            _httpClient = httpService.GetClientAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 使用AI分析看板娘的回复并自动记录相关信息
        /// </summary>
        /// <param name="reply">看板娘的回复内容</param>
        public async Task AnalyzeAndRecordSelfInfoAsync(string reply)
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
                var analysisPrompt = $@"你需要分析看板娘的最新回复，与现有记忆对比，识别需要新增、更新或删除的信息。

【看板娘现有记忆】：
{existingMemory}

【看板娘最新回复】：
{reply}

请分析最新回复，识别所有需要的数据操作，以JSON格式返回：

{{
  ""states"": [
    {{
      ""content"": ""状态描述"",
      ""type"": ""状态类型(physical/emotional/activity/mental)"",
      ""duration"": 预计持续时间(分钟，0表示瞬时状态),
      ""operation"": ""操作类型(add/update/remove)"",
      ""targetContent"": ""要更新或删除的目标状态(仅update/remove时需要)""
    }}
  ],
  ""traits"": [
    {{
      ""content"": ""特征或偏好描述"",
      ""type"": ""类型(preference/skill/limitation/personality)"",
      ""intensity"": 强度(1-5),
      ""operation"": ""操作类型(add/update/remove)"",
      ""targetContent"": ""要更新或删除的目标特征(仅update/remove时需要)""
    }}
  ],
  ""commitments"": [
    {{
      ""content"": ""承诺或计划描述"",
      ""type"": ""类型(intention/promise/plan)"",
      ""expectedTime"": ""预期时间(ISO格式，可为null)"",
      ""operation"": ""操作类型(add/update/complete/cancel)"",
      ""targetContent"": ""要操作的目标承诺(仅update/complete/cancel时需要)"",
      ""newStatus"": ""新状态(fulfilled/cancelled/pending，仅状态更新时需要)""
    }}
  ],
  ""actions"": [
    {{
      ""actionType"": ""管理操作类型(cleanup_expired/update_status等)"",
      ""target"": ""操作目标"",
      ""parameters"": ""操作参数""
    }}
  ]
}}

**智能分析原则**：
1. **新增(add)**：全新的状态、特征、承诺
2. **更新(update)**：现有信息的修改，如状态变化、特征强度调整
3. **移除(remove)**：明确表示不再适用的信息
4. **完成(complete)**：承诺已履行，如'我去睡觉了'→之前'我要去睡觉'的承诺
5. **取消(cancel)**：放弃的计划或承诺
6. **智能状态转换**：识别状态的自然过渡，如'累了'→'休息了'→'精神了'
7. **过期检测**：识别明显过期的临时状态
8. **语义去重**：相同含义的信息合并处理
9. **如果没有任何操作需求，所有数组都返回空**
10. 只返回JSON，不要其他解释文字";

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
        /// 智能处理AI分析结果，支持增删改操作
        /// </summary>
        /// <param name="analysisResult">AI分析结果</param>
        private async Task ProcessAnalysisResultDirectAsync(SelfAnalysisResult analysisResult)
        {
            try
            {
                // 处理状态信息
                if (analysisResult.States != null)
                {
                    foreach (var state in analysisResult.States)
                    {
                        if (string.IsNullOrWhiteSpace(state.Content)) continue;

                        switch (state.Operation.ToLower())
                        {
                            case "add":
                                await _selfMemoryService.RecordSelfStateAsync(state.Content, state.Type, state.Duration);
                                _logger.LogInformation("AI新增状态：{content}，类型：{type}", state.Content, state.Type);
                                break;

                            case "update":
                                // 先删除旧状态，再添加新状态（简化处理）
                                if (!string.IsNullOrWhiteSpace(state.TargetContent))
                                {
                                    await _selfMemoryService.RemoveSelfStateAsync(state.TargetContent);
                                    await _selfMemoryService.RecordSelfStateAsync(state.Content, state.Type, state.Duration);
                                    _logger.LogInformation("AI更新状态：{oldContent} → {newContent}", state.TargetContent, state.Content);
                                }
                                break;

                            case "remove":
                                await _selfMemoryService.RemoveSelfStateAsync(state.TargetContent ?? state.Content);
                                _logger.LogInformation("AI移除状态：{content}", state.TargetContent ?? state.Content);
                                break;
                        }
                    }
                }

                // 处理特征和偏好
                if (analysisResult.Traits != null)
                {
                    foreach (var trait in analysisResult.Traits)
                    {
                        if (string.IsNullOrWhiteSpace(trait.Content)) continue;

                        switch (trait.Operation.ToLower())
                        {
                            case "add":
                                await _selfMemoryService.RecordSelfTraitAsync(trait.Content, trait.Type, trait.Intensity);
                                _logger.LogInformation("AI新增特征：{content}，类型：{type}", trait.Content, trait.Type);
                                break;

                            case "update":
                                if (!string.IsNullOrWhiteSpace(trait.TargetContent))
                                {
                                    await _selfMemoryService.RemoveSelfTraitAsync(trait.TargetContent);
                                    await _selfMemoryService.RecordSelfTraitAsync(trait.Content, trait.Type, trait.Intensity);
                                    _logger.LogInformation("AI更新特征：{oldContent} → {newContent}", trait.TargetContent, trait.Content);
                                }
                                break;

                            case "remove":
                                await _selfMemoryService.RemoveSelfTraitAsync(trait.TargetContent ?? trait.Content);
                                _logger.LogInformation("AI移除特征：{content}", trait.TargetContent ?? trait.Content);
                                break;
                        }
                    }
                }

                // 处理承诺和计划
                if (analysisResult.Commitments != null)
                {
                    foreach (var commitment in analysisResult.Commitments)
                    {
                        if (string.IsNullOrWhiteSpace(commitment.Content)) continue;

                        DateTime? expectedTime = null;
                        if (!string.IsNullOrWhiteSpace(commitment.ExpectedTime) &&
                            DateTime.TryParse(commitment.ExpectedTime, out var parsedTime))
                        {
                            expectedTime = parsedTime;
                        }

                        switch (commitment.Operation.ToLower())
                        {
                            case "add":
                                await _selfMemoryService.MakeCommitmentAsync(commitment.Content, commitment.Type, expectedTime);
                                _logger.LogInformation("AI新增承诺：{content}，类型：{type}", commitment.Content, commitment.Type);
                                break;

                            case "update":
                                if (!string.IsNullOrWhiteSpace(commitment.TargetContent))
                                {
                                    await _selfMemoryService.UpdateCommitmentStatusAsync(commitment.TargetContent, "cancelled");
                                    await _selfMemoryService.MakeCommitmentAsync(commitment.Content, commitment.Type, expectedTime);
                                    _logger.LogInformation("AI更新承诺：{oldContent} → {newContent}", commitment.TargetContent, commitment.Content);
                                }
                                break;

                            case "complete":
                                await _selfMemoryService.UpdateCommitmentStatusAsync(commitment.TargetContent ?? commitment.Content, "fulfilled");
                                _logger.LogInformation("AI完成承诺：{content}", commitment.TargetContent ?? commitment.Content);
                                break;

                            case "cancel":
                                await _selfMemoryService.UpdateCommitmentStatusAsync(commitment.TargetContent ?? commitment.Content, "cancelled");
                                _logger.LogInformation("AI取消承诺：{content}", commitment.TargetContent ?? commitment.Content);
                                break;
                        }
                    }
                }

                // 处理管理操作
                if (analysisResult.Actions != null)
                {
                    foreach (var action in analysisResult.Actions)
                    {
                        await ProcessSelfManagementActionAsync(action);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理AI分析结果失败");
            }
        }

        /// <summary>
        /// 处理自我管理操作
        /// </summary>
        /// <param name="action">管理操作</param>
        private async Task ProcessSelfManagementActionAsync(SelfAnalysisAction action)
        {
            try
            {
                switch (action.ActionType.ToLower())
                {
                    case "cleanup_expired":
                        await _selfMemoryService.CleanupExpiredStatesAsync();
                        _logger.LogInformation("AI触发过期状态清理");
                        break;

                    case "update_status":
                        // 批量状态更新等操作
                        _logger.LogInformation("AI触发状态更新操作：{target}", action.Target);
                        break;

                    default:
                        _logger.LogWarning("未知的管理操作类型：{actionType}", action.ActionType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理自我管理操作失败：{actionType}", action.ActionType);
            }
        }

        /// <summary>
        /// 执行定期数据生命周期管理
        /// </summary>
        public async Task PerformPeriodicLifecycleManagementAsync()
        {
            try
            {
                _logger.LogInformation("开始执行看板娘定期数据生命周期管理");

                // 执行自动生命周期管理
                await _selfMemoryService.PerformLifecycleManagementAsync();

                // 清理过期状态
                await _selfMemoryService.CleanupExpiredStatesAsync();

                // 清理已完成的承诺
                await _selfMemoryService.CleanupCompletedCommitmentsAsync();

                _logger.LogInformation("看板娘定期数据生命周期管理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "看板娘定期数据生命周期管理失败");
            }
        }

        /// <summary>
        /// 智能状态转换和清理
        /// </summary>
        public async Task PerformIntelligentStateManagementAsync()
        {
            try
            {
                _logger.LogInformation("开始执行看板娘智能状态管理");

                // 获取当前记忆状态
                var currentMemory = await _selfMemoryService.GetFormattedMemoryAsync("all");

                // 使用AI分析当前状态，识别需要转换或清理的内容
                var intelligentAnalysisPrompt = $@"你需要分析看板娘的当前记忆状态，识别需要智能管理的内容。

【当前记忆状态】：
{currentMemory}

请分析当前状态，识别以下管理需求，以JSON格式返回：

{{
  ""states"": [
    {{
      ""content"": ""需要转换的状态"",
      ""type"": ""状态类型"",
      ""duration"": 0,
      ""operation"": ""remove"",
      ""reason"": ""转换原因（如：已完成、矛盾、过时等）""
    }}
  ],
  ""traits"": [
    {{
      ""content"": ""需要调整的特征"",
      ""type"": ""特征类型"",
      ""intensity"": 新强度值,
      ""operation"": ""update"",
      ""reason"": ""调整原因""
    }}
  ],
  ""commitments"": [
    {{
      ""content"": ""需要处理的承诺"",
      ""operation"": ""complete或cancel"",
      ""reason"": ""处理原因""
    }}
  ],
  ""actions"": [
    {{
      ""actionType"": ""cleanup_expired"",
      ""reason"": ""清理原因""
    }}
  ]
}}

**智能管理原则**：
1. **自然状态衰减**：识别应该自然结束的状态
2. **矛盾检测**：发现相互冲突的状态或特征
3. **完成推断**：基于上下文推断可能已完成的承诺
4. **强度调整**：根据时间和频率调整特征强度
5. **数据一致性**：保持记忆的逻辑一致性
6. **如果没有需要管理的内容，所有数组都返回空**
7. 只返回JSON，不要其他解释文字";

                var analysisMessages = new List<ChatCompletionMessage>
                {
                    new() { Role = "system", Content = "你是一个专门进行智能数据管理的AI助手，专注于维护数据的一致性和时效性。" },
                    new() { Role = "user", Content = intelligentAnalysisPrompt }
                };

                var analysisRequest = new ChatCompletionModel
                {
                    Model = string.IsNullOrWhiteSpace(_configuration["ChatGPTModel"]) ? "gpt-3.5-turbo" : _configuration["ChatGPTModel"]!,
                    Messages = analysisMessages,
                    temperature = 0.1
                };

                var url = _configuration["ChatGPTApiUrl"];
                var analysisResponse = await _httpClient.PostAsJsonAsync(url + "v1/chat/completions", analysisRequest);

                if (analysisResponse.IsSuccessStatusCode)
                {
                    var analysisContent = await analysisResponse.Content.ReadAsStringAsync();
                    var analysisResult = JsonSerializer.Deserialize<ChatResult>(analysisContent, _jsonOptions);
                    var content = analysisResult?.Choices?.FirstOrDefault()?.Message?.Content;

                    if (!string.IsNullOrWhiteSpace(content))
                    {
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

                        var managementResult = JsonSerializer.Deserialize<SelfAnalysisResult>(jsonContent, _jsonOptions);
                        if (managementResult != null)
                        {
                            await ProcessAnalysisResultDirectAsync(managementResult);
                            _logger.LogInformation("智能状态管理完成，处理了状态转换和清理");
                        }
                    }
                }

                _logger.LogInformation("看板娘智能状态管理完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "看板娘智能状态管理失败");
            }
        }
    }
}
