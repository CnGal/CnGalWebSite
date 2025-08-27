using CnGalWebSite.Core.Services;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Models.SelfAnalysis;
using CnGalWebSite.Kanban.ChatGPT.Models.UserProfile;
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
        /// 【优化版】智能融合分析看板娘回复，减少冗余数据
        /// </summary>
        /// <param name="reply">看板娘的回复内容</param>
        public async Task AnalyzeAndFuseSelfInfoAsync(string reply)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reply))
                    return;

                // 获取现有记忆
                var existingMemory = await _selfMemoryService.GetSelfMemoryAsync();

                // 【核心改进】使用AI进行智能融合而非简单追加
                var fusedMemory = await FuseSelfDataWithAIAsync(reply, existingMemory);

                if (fusedMemory != null)
                {
                    // 直接替换完整记忆
                    await ReplaceSelfMemoryAsync(fusedMemory);
                }

                _logger.LogInformation("AI智能融合看板娘记忆完成，内容长度：{length}", reply.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI智能融合看板娘记忆失败：{error}", ex.Message);
            }
        }

        /// <summary>
        /// 【核心方法】使用AI进行智能记忆融合，输出精简的完整记忆
        /// </summary>
        /// <param name="reply">看板娘的回复</param>
        /// <param name="existingMemory">现有记忆</param>
        /// <returns>融合后的完整记忆</returns>
        private async Task<KanbanSelfMemoryModel?> FuseSelfDataWithAIAsync(string reply, KanbanSelfMemoryModel existingMemory)
        {
            try
            {
                var existingJson = JsonSerializer.Serialize(existingMemory, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                var fusePrompt = $@"你需要基于看板娘的最新回复，输出一个精简且高质量的记忆档案。要求智能融合、去重、清理过期数据。

【看板娘现有记忆】：
{existingJson}

【看板娘最新回复】：
{reply}

【融合要求】：
1. **语义去重**：合并意思相同的状态和特征
2. **数据精简**：只保留最重要和最新的信息
3. **过期清理**：删除明显过时或矛盾的状态
4. **状态管理**：合理设置状态的活跃性和持续时间
5. **数量控制**：
   - personal_states: 最多30个（保留最近和活跃的）
   - personal_traits: 最多25个（保留高强度的）
   - commitments: 最多20个（清理已完成的旧承诺）
   - conversation_memories: 最多50个（保留高重要度的）

请输出完整的融合后记忆（JSON格式）：

{{
  ""memory_id"": ""{existingMemory.MemoryId}"",
  ""personal_states"": [
    {{
      ""state"": ""状态描述"",
      ""state_type"": ""状态类型"",
      ""is_active"": true/false,
      ""recorded_at"": ""时间"",
      ""duration_minutes"": 持续时间
    }}
  ],
  ""personal_traits"": [
    {{
      ""trait"": ""特征描述"",
      ""trait_type"": ""特征类型"",
      ""intensity"": 强度1-5,
      ""recorded_at"": ""时间""
    }}
  ],
  ""commitments"": [
    {{
      ""commitment"": ""承诺内容"",
      ""commitment_type"": ""承诺类型"",
      ""status"": ""状态"",
      ""expected_time"": ""预期时间"",
      ""recorded_at"": ""记录时间""
    }}
  ],
  ""conversation_memories"": [
    {{
      ""content"": ""记忆内容"",
      ""memory_type"": ""记忆类型"",
      ""related_user_id"": ""相关用户ID"",
      ""importance"": 重要度1-5,
      ""recorded_at"": ""时间""
    }}
  ],
  ""created_at"": ""{existingMemory.CreatedAt:yyyy-MM-ddTHH:mm:sszzz}"",
  ""updated_at"": ""{DateTime.Now:yyyy-MM-ddTHH:mm:sszzz}""
}}

**重要**：只返回JSON，不要其他解释文字。确保记忆质量高且无冗余。";

                var fusionMessages = new List<ChatCompletionMessage>
                {
                    new() { Role = "system", Content = "你是一个专业的记忆融合分析师，专注于输出高质量、无冗余的看板娘记忆。" },
                    new() { Role = "user", Content = fusePrompt }
                };

                var fusionRequest = new ChatCompletionModel
                {
                    Model = string.IsNullOrWhiteSpace(_configuration["ChatGPTModel"]) ? "gpt-3.5-turbo" : _configuration["ChatGPTModel"]!,
                    Messages = fusionMessages,
                    temperature = 0.1
                };

                var url = _configuration["ChatGPTApiUrl"];
                var fusionResponse = await _httpClient.PostAsJsonAsync(url + "v1/chat/completions", fusionRequest);

                if (!fusionResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI记忆融合请求失败：{status}", fusionResponse.StatusCode);
                    return null;
                }

                var fusionContent = await fusionResponse.Content.ReadAsStringAsync();
                var fusionResult = JsonSerializer.Deserialize<ChatResult>(fusionContent, _jsonOptions);

                var content = fusionResult?.Choices?.FirstOrDefault()?.Message?.Content;

                if (content == null)
                {
                    _logger.LogWarning("AI记忆融合响应为空");
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

                var fusedMemory = JsonSerializer.Deserialize<KanbanSelfMemoryModel>(jsonContent, _jsonOptions);
                return fusedMemory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI记忆融合失败");
                return null;
            }
        }

        /// <summary>
        /// 替换完整的看板娘记忆
        /// </summary>
        /// <param name="fusedMemory">融合后的记忆</param>
        private async Task ReplaceSelfMemoryAsync(KanbanSelfMemoryModel fusedMemory)
        {
            try
            {
                // 确保基本信息正确
                fusedMemory.UpdatedAt = DateTime.Now;

                // 【实现注释的逻辑】完整替换看板娘记忆
                // 由于SelfMemoryService可能没有直接替换方法，我们通过JSON序列化保存融合后的完整记忆
                var fusedMemoryJson = JsonSerializer.Serialize(fusedMemory, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                });

                // 【完整实现】直接替换看板娘的完整记忆
                var replaceResult = await _selfMemoryService.ReplaceCompleteMemoryAsync(fusedMemory);

                if (replaceResult)
                {
                    _logger.LogInformation("看板娘记忆智能融合并完整替换成功！数据已精简。状态数：{states}，特征数：{traits}，承诺数：{commitments}，对话记忆数：{conversations}",
                        fusedMemory.PersonalStates?.Count ?? 0,
                        fusedMemory.PersonalTraits?.Count ?? 0,
                        fusedMemory.Commitments?.Count ?? 0,
                        fusedMemory.ConversationMemories?.Count ?? 0);
                }
                else
                {
                    _logger.LogWarning("看板娘记忆完整替换失败，回退到生命周期管理模式");
                    await _selfMemoryService.PerformLifecycleManagementAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "替换看板娘记忆失败");
            }
        }
    }
}
