using CnGalWebSite.Core.Services;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Models.UserAnalysis;
using CnGalWebSite.Kanban.ChatGPT.Models.UserProfile;
using CnGalWebSite.Kanban.ChatGPT.Services.ChatGPTService;
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

namespace CnGalWebSite.Kanban.ChatGPT.Services.UserAnalysisService
{
    /// <summary>
    /// 用户AI分析服务实现
    /// </summary>
    public class UserAnalysisService : IUserAnalysisService
    {
        private readonly ILogger<UserAnalysisService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IUserProfileService _userProfileService;
        private readonly IFunctionCallingService _functionCallingService;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public UserAnalysisService(ILogger<UserAnalysisService> logger, IConfiguration configuration,
            IHttpService httpService, IUserProfileService userProfileService, IFunctionCallingService functionCallingService)
        {
            _logger = logger;
            _configuration = configuration;
            _userProfileService = userProfileService;
            _functionCallingService = functionCallingService;

            _httpClient = httpService.GetClientAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// 【优化版】智能融合分析用户信息，输出融合后的完整档案而非增量操作
        /// </summary>
        /// <param name="messages">对话消息列表</param>
        public async Task AnalyzeAndFuseUserInfoAsync(List<ChatCompletionMessage> messages)
        {
            try
            {
                // 只分析用户的最新消息
                var userMessage = messages.LastOrDefault(m => m.Role == "user");
                var assistantMessage = messages.LastOrDefault(m => m.Role == "assistant");

                if (userMessage == null || string.IsNullOrWhiteSpace(userMessage.Content))
                    return;

                // 提取用户ID
                var userId = ExtractUserIdFromMessage(userMessage.Content);
                if (string.IsNullOrWhiteSpace(userId))
                    return;

                // 获取用户现有档案
                var profile = await _userProfileService.GetUserProfileAsync(userId);

                // 【核心改进】使用AI进行智能融合而非增量更新
                var fusedProfile = await FuseUserDataWithAIAsync(userMessage.Content,
                    assistantMessage?.Content ?? "", profile, userId);

                if (fusedProfile != null)
                {
                    // 直接替换完整档案
                    await ReplaceCompleteUserProfileAsync(fusedProfile, userId);
                }

                _logger.LogInformation("AI智能融合用户档案完成，用户ID：{userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI智能融合用户档案失败：{error}", ex.Message);
            }
        }

        // 【已删除】旧的AnalyzeAndRecordUserInfoAsync方法，现在使用智能融合方法

        /// <summary>
        /// 从消息中提取用户ID
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>用户ID</returns>
        public string ExtractUserIdFromMessage(string content)
        {
            if (content.Contains("[UserID:") && content.Contains("]"))
            {
                var start = content.IndexOf("[UserID:") + 8;
                var end = content.IndexOf("]", start);
                if (end > start)
                {
                    var userId = content.Substring(start, end - start);
                    if (long.TryParse(userId, out _))
                    {
                        return userId;
                    }
                }
            }
            return "";
        }

        // 【已删除】旧的AnalyzeUserMessageWithAIAsync方法（约130行代码）

        // 【已删除】旧的ProcessUserAnalysisResultAsync方法（约190行代码）
        // 【已删除】旧的ProcessUserManagementActionAsync方法（约30行代码）

        /// <summary>
        /// 执行用户数据生命周期管理
        /// </summary>
        /// <param name="userId">用户ID</param>
        public async Task PerformUserLifecycleManagementAsync(string userId)
        {
            try
            {
                _logger.LogInformation("开始执行用户数据生命周期管理，用户ID：{userId}", userId);

                var profile = await _userProfileService.GetUserProfileAsync(userId);

                bool hasChanges = false;
                var now = DateTime.Now;

                // 1. 清理已完成的上下文（超过30天）
                var completedContextsToRemove = profile.CurrentContexts
                    .Where(c => c.Status == "completed" && c.UpdatedAt < now.AddDays(-30))
                    .ToList();

                foreach (var context in completedContextsToRemove)
                {
                    profile.CurrentContexts.Remove(context);
                    hasChanges = true;
                }

                // 2. 标记长期未活跃的上下文为暂停状态
                var inactiveContexts = profile.CurrentContexts
                    .Where(c => c.Status == "active" && c.UpdatedAt < now.AddDays(-14))
                    .ToList();

                foreach (var context in inactiveContexts)
                {
                    context.Status = "paused";
                    context.UpdatedAt = now;
                    hasChanges = true;
                }

                // 3. 清理低可信度的行为观察（保留最近和高可信度的）
                if (profile.BehaviorObservations.Count > 20)
                {
                    var observationsToKeep = profile.BehaviorObservations
                        .Where(b => b.Confidence >= 4 || b.ObservedAt > now.AddDays(-7))
                        .OrderByDescending(b => b.Confidence)
                        .ThenByDescending(b => b.ObservedAt)
                        .Take(15)
                        .ToList();

                    if (observationsToKeep.Count < profile.BehaviorObservations.Count)
                    {
                        profile.BehaviorObservations = observationsToKeep;
                        hasChanges = true;
                    }
                }

                // 4. 清理过时的偏好（负面偏好且记录时间超过60天的）
                foreach (var preferenceGroup in profile.Preferences.ToList())
                {
                    var preferencesToRemove = preferenceGroup.Value
                        .Where(p => !p.IsPositive && p.RecordedAt < now.AddDays(-60))
                        .ToList();

                    foreach (var preference in preferencesToRemove)
                    {
                        preferenceGroup.Value.Remove(preference);
                        hasChanges = true;
                    }

                    // 如果某个类型的偏好为空，移除整个类型
                    if (!preferenceGroup.Value.Any())
                    {
                        profile.Preferences.Remove(preferenceGroup.Key);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    profile.UpdatedAt = now;
                    await _userProfileService.UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
                    {
                        Field = "lifecycle_cleanup",
                        Value = "automated_cleanup",
                        Operation = "set"
                    });

                    _logger.LogInformation("用户数据生命周期管理完成，用户ID：{userId}，清理了过期数据", userId);
                }
                else
                {
                    _logger.LogInformation("用户数据生命周期管理完成，用户ID：{userId}，无需清理", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户数据生命周期管理失败，用户ID：{userId}", userId);
            }
        }

        /// <summary>
        /// 批量执行所有用户的数据生命周期管理
        /// </summary>
        public async Task PerformBatchUserLifecycleManagementAsync()
        {
            try
            {
                _logger.LogInformation("开始批量执行用户数据生命周期管理");

                // 注意：这里需要根据实际的用户存储机制来获取所有用户ID
                // 由于当前的IUserProfileService接口没有提供获取所有用户的方法
                // 这里仅做示例实现，实际使用时需要添加相应的方法

                _logger.LogInformation("批量用户数据生命周期管理完成（当前为示例实现）");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量用户数据生命周期管理失败");
            }
        }
        // 【已删除】PerformIntelligentUserDataManagementAsync方法（约125行代码）- 没有被引用

        /// <summary>
        /// 【核心方法】使用AI进行智能数据融合，输出精简的完整档案
        /// </summary>
        /// <param name="userMessage">用户消息</param>
        /// <param name="assistantMessage">助手回复</param>
        /// <param name="existingProfile">现有档案</param>
        /// <param name="userId">用户ID</param>
        /// <returns>融合后的完整档案</returns>
        private async Task<UserProfileModel?> FuseUserDataWithAIAsync(string userMessage, string assistantMessage,
            UserProfileModel existingProfile, string userId)
        {
            try
            {
                var existingJson = JsonSerializer.Serialize(existingProfile, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                var fusePrompt = $@"你需要基于新的对话，输出一个精简且高质量的用户档案。要求智能融合、去重、清理过期数据。

【用户现有档案】：
{existingJson}

【新对话】：
用户：{userMessage}
助手：{assistantMessage}

【融合要求】：
1. **语义去重**：合并意思相同的内容（如'galgame'和'美少女游戏'）
2. **数据精简**：只保留最重要和最新的信息
3. **过期清理**：删除明显过时或矛盾的数据
4. **优先级排序**：重要信息排在前面
5. **数量控制**：
   - interests: 最多15个
   - preferences每类: 最多10个
   - behavior_observations: 最多20个
   - current_contexts: 最多15个

请输出完整的融合后档案（JSON格式）：

{{
  ""user_id"": ""{userId}"",
  ""nickname"": ""昵称（如有更新）"",
  ""personality"": ""性格描述（融合后的精简版）"",
  ""interests"": [""精选的兴趣列表""],
  ""preferred_game_types"": [""游戏类型偏好""],
  ""communication_style"": ""沟通风格"",
  ""age_group"": ""年龄段"",
  ""special_preferences"": {{""key"": ""value""}},
  ""preferences"": {{
    ""类型"": [
      {{
      ""content"": ""偏好内容"",
        ""is_positive"": true/false,
        ""recorded_at"": ""2025-01-27T12:00:00+08:00"",
        ""intensity"": 强度1-5
      }}
    ]
  }},
  ""behavior_observations"": [
    {{
      ""behavior_type"": ""行为类型"",
      ""observation"": ""观察内容"",
      ""observed_at"": ""时间"",
      ""confidence"": 可信度1-5
    }}
  ],
  ""current_contexts"": [
    {{
      ""context_type"": ""上下文类型"",
      ""content"": ""内容"",
      ""status"": ""状态"",
      ""created_at"": ""创建时间"",
      ""updated_at"": ""更新时间""
    }}
  ],
  ""created_at"": ""{existingProfile.CreatedAt:yyyy-MM-ddTHH:mm:sszzz}"",
  ""updated_at"": ""{DateTime.Now:yyyy-MM-ddTHH:mm:sszzz}""
}}

**重要**：只返回JSON，不要其他解释文字。确保数据质量高且无冗余。";

                var fusionMessages = new List<ChatCompletionMessage>
                {
                    new() { Role = "system", Content = "你是一个专业的数据融合分析师，专注于输出高质量、无冗余的用户档案。" },
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
                    _logger.LogWarning("AI数据融合请求失败：{status}", fusionResponse.StatusCode);
                    return null;
                }

                var fusionContent = await fusionResponse.Content.ReadAsStringAsync();
                var fusionResult = JsonSerializer.Deserialize<ChatResult>(fusionContent, _jsonOptions);

                var content = fusionResult?.Choices?.FirstOrDefault()?.Message?.Content;

                if (content == null)
                {
                    _logger.LogWarning("AI数据融合响应为空");
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

                var fusedProfile = JsonSerializer.Deserialize<UserProfileModel>(jsonContent, _jsonOptions);
                return fusedProfile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI数据融合失败");
                return null;
            }
        }

        /// <summary>
        /// 替换完整的用户档案
        /// </summary>
        /// <param name="fusedProfile">融合后的档案</param>
        /// <param name="userId">用户ID</param>
        private async Task ReplaceCompleteUserProfileAsync(UserProfileModel fusedProfile, string userId)
        {
            try
            {
                // 确保基本信息正确
                fusedProfile.UserId = userId;
                fusedProfile.UpdatedAt = DateTime.Now;

                // 使用内部API直接替换整个档案
                var profileJson = JsonSerializer.Serialize(fusedProfile, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                // 通过特殊的内部更新操作来替换整个档案
                await _userProfileService.UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
                {
                    Field = "complete_profile_replace",
                    Value = profileJson,
                    Operation = "set"
                });

                _logger.LogInformation("成功替换用户完整档案，用户ID：{userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "替换用户档案失败，用户ID：{userId}", userId);
            }
        }
    }
}
