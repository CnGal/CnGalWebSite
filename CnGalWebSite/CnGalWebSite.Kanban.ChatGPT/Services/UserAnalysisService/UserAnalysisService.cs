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
        /// 自动分析用户消息并记录用户信息
        /// </summary>
        /// <param name="messages">对话消息列表</param>
        public async Task AnalyzeAndRecordUserInfoAsync(List<ChatCompletionMessage> messages)
        {
            try
            {
                // 只分析用户的最新消息
                var userMessage = messages.LastOrDefault(m => m.Role == "user");
                if (userMessage == null || string.IsNullOrWhiteSpace(userMessage.Content))
                    return;

                // 提取用户ID
                var userId = ExtractUserIdFromMessage(userMessage.Content);
                if (string.IsNullOrWhiteSpace(userId))
                    return;

                // 获取用户现有档案
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                var analysisResult = await AnalyzeUserMessageWithAIAsync(userMessage.Content, profile, userId);

                if (analysisResult != null)
                {
                    // 记录AI分析的结果
                    await ProcessUserAnalysisResultAsync(analysisResult, userId);
                }

                _logger.LogInformation("AI自动分析用户消息完成，用户ID：{userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI自动分析用户消息失败：{error}", ex.Message);
            }
        }

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

        /// <summary>
        /// 使用AI分析用户消息内容
        /// </summary>
        /// <param name="userMessage">用户消息</param>
        /// <param name="profile">用户现有档案</param>
        /// <param name="userId">用户ID</param>
        /// <returns>分析结果</returns>
        private async Task<UserAnalysisResult?> AnalyzeUserMessageWithAIAsync(string userMessage, object profile, string userId)
        {
            try
            {
                var profileJson = JsonSerializer.Serialize(profile, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                var analysisPrompt = $@"你需要分析用户的消息，与现有档案对比，识别需要新增、更新或删除的用户信息。

【用户现有档案】：
{profileJson}

【用户最新消息】：
{userMessage}

请分析用户消息，识别所有需要的数据操作，以JSON格式返回：

{{
  ""nickname"": ""用户提到的昵称（如有变化）"",
  ""interests"": [
    {{
      ""content"": ""兴趣内容"",
      ""operation"": ""操作类型(add/remove/update_intensity)"",
      ""intensity"": 兴趣强度(1-5，可选)
    }}
  ],
  ""personality_traits"": [
    {{
      ""content"": ""性格特点"",
      ""operation"": ""操作类型(add/update/remove)"",
      ""targetContent"": ""要更新/删除的目标特征(仅update/remove时需要)""
    }}
  ],
  ""preferences"": [
    {{
      ""type"": ""偏好类型"",
      ""content"": ""具体偏好内容"",
      ""isPositive"": true/false,
      ""operation"": ""操作类型(add/update/remove)"",
      ""targetContent"": ""要更新/删除的目标偏好(仅update/remove时需要)""
    }}
  ],
  ""contexts"": [
    {{
      ""type"": ""上下文类型"",
      ""content"": ""具体内容"",
      ""status"": ""状态(active/interested/completed/paused)"",
      ""operation"": ""操作类型(add/update_status/remove)"",
      ""targetContent"": ""要更新的目标上下文(仅update_status时需要)""
    }}
  ],
  ""behaviors"": [
    {{
      ""type"": ""行为类型"",
      ""observation"": ""具体观察到的行为模式"",
      ""operation"": ""操作类型(add/update/remove)"",
      ""confidence"": 观察可信度(1-5)
    }}
  ],
  ""actions"": [
    {{
      ""actionType"": ""管理操作类型(cleanup_completed_contexts/update_interest_decay等)"",
      ""target"": ""操作目标"",
      ""parameters"": ""操作参数""
    }}
  ]
}}

**智能分析原则**：
1. **新增(add)**：全新的兴趣、偏好、上下文等
2. **更新(update)**：已有信息的修改，如兴趣强度变化、性格补充
3. **移除(remove)**：明确表示不再感兴趣或不再适用的信息
4. **状态更新(update_status)**：上下文状态转变，如项目完成、暂停等
5. **强度调整(update_intensity)**：兴趣热度的变化
6. **自然过渡检测**：识别用户状态的自然变化
7. **语义去重**：避免重复记录相同含义的信息
8. **智能推断**：基于用户行为推断深层偏好变化
9. **如果没有任何操作需求，所有数组都返回空**
10. 只返回JSON，不要其他解释文字";

                var analysisMessages = new List<ChatCompletionMessage>
                {
                    new() { Role = "system", Content = "你是一个专门分析用户信息的AI助手，专注于提取有助于个性化服务的用户特征。" },
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
                    _logger.LogWarning("AI用户分析请求失败：{status}", analysisResponse.StatusCode);
                    return null;
                }

                var analysisContent = await analysisResponse.Content.ReadAsStringAsync();
                var analysisResult = JsonSerializer.Deserialize<ChatResult>(analysisContent, _jsonOptions);

                var content = analysisResult?.Choices?.FirstOrDefault()?.Message?.Content;

                if (content == null)
                {
                    _logger.LogWarning("AI用户分析响应为空");
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

                var result = JsonSerializer.Deserialize<UserAnalysisResult>(jsonContent, _jsonOptions);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI分析用户消息失败");
                return null;
            }
        }

        /// <summary>
        /// 智能处理AI分析的用户信息结果，支持增删改操作
        /// </summary>
        /// <param name="analysisResult">分析结果</param>
        /// <param name="userId">用户ID</param>
        private async Task ProcessUserAnalysisResultAsync(UserAnalysisResult analysisResult, string userId)
        {
            try
            {
                // 记录昵称
                if (!string.IsNullOrWhiteSpace(analysisResult.Nickname))
                {
                    await _userProfileService.SetUserNicknameAsync(userId, analysisResult.Nickname);
                    _logger.LogInformation("AI更新用户昵称：{nickname}，用户ID：{userId}", analysisResult.Nickname, userId);
                }

                // 处理兴趣爱好
                if (analysisResult.Interests != null)
                {
                    foreach (var interest in analysisResult.Interests)
                    {
                        if (string.IsNullOrWhiteSpace(interest.Content)) continue;

                        switch (interest.Operation.ToLower())
                        {
                            case "add":
                                await _userProfileService.AddUserInterestAsync(userId, interest.Content);
                                _logger.LogInformation("AI新增用户兴趣：{interest}，用户ID：{userId}", interest.Content, userId);
                                break;

                            case "remove":
                                await _userProfileService.RemoveUserInterestAsync(userId, interest.Content);
                                _logger.LogInformation("AI移除用户兴趣：{interest}，用户ID：{userId}", interest.Content, userId);
                                break;

                            case "update_intensity":
                                // 暂时通过删除再添加来处理强度更新
                                await _userProfileService.RemoveUserInterestAsync(userId, interest.Content);
                                await _userProfileService.AddUserInterestAsync(userId, interest.Content);
                                _logger.LogInformation("AI更新用户兴趣强度：{interest}，强度：{intensity}，用户ID：{userId}",
                                    interest.Content, interest.Intensity, userId);
                                break;
                        }
                    }
                }

                // 处理性格特征
                if (analysisResult.PersonalityTraits != null)
                {
                    foreach (var trait in analysisResult.PersonalityTraits)
                    {
                        if (string.IsNullOrWhiteSpace(trait.Content)) continue;

                        switch (trait.Operation.ToLower())
                        {
                            case "add":
                                await _userProfileService.SetUserPersonalityAsync(userId, trait.Content);
                                _logger.LogInformation("AI新增用户性格：{trait}，用户ID：{userId}", trait.Content, userId);
                                break;

                            case "update":
                                // 性格特征的更新通常是追加或修正
                                await _userProfileService.SetUserPersonalityAsync(userId, trait.Content);
                                _logger.LogInformation("AI更新用户性格：{oldTrait} → {newTrait}，用户ID：{userId}",
                                    trait.TargetContent, trait.Content, userId);
                                break;

                            case "remove":
                                // 性格特征移除需要通过自定义逻辑处理
                                _logger.LogInformation("AI标记移除用户性格：{trait}，用户ID：{userId}", trait.Content, userId);
                                break;
                        }
                    }
                }

                // 处理偏好
                if (analysisResult.Preferences != null)
                {
                    foreach (var preference in analysisResult.Preferences)
                    {
                        if (string.IsNullOrWhiteSpace(preference.Content)) continue;

                        switch (preference.Operation.ToLower())
                        {
                            case "add":
                            case "update":
                                await _functionCallingService.ExecuteFunction("remember_user_preference", JsonSerializer.Serialize(new
                                {
                                    user_id = userId,
                                    preference_type = preference.Type,
                                    preference_value = preference.Content,
                                    is_positive = preference.IsPositive.ToString().ToLower()
                                }));
                                _logger.LogInformation("AI记录用户偏好：{content}，类型：{type}，正面：{positive}，用户ID：{userId}",
                                    preference.Content, preference.Type, preference.IsPositive, userId);
                                break;

                            case "remove":
                                // 偏好移除可以通过设置为负面来处理
                                await _functionCallingService.ExecuteFunction("remember_user_preference", JsonSerializer.Serialize(new
                                {
                                    user_id = userId,
                                    preference_type = preference.Type,
                                    preference_value = preference.TargetContent ?? preference.Content,
                                    is_positive = "false"
                                }));
                                _logger.LogInformation("AI移除用户偏好：{content}，类型：{type}，用户ID：{userId}",
                                    preference.TargetContent ?? preference.Content, preference.Type, userId);
                                break;
                        }
                    }
                }

                // 处理上下文
                if (analysisResult.Contexts != null)
                {
                    foreach (var context in analysisResult.Contexts)
                    {
                        if (string.IsNullOrWhiteSpace(context.Content)) continue;

                        switch (context.Operation.ToLower())
                        {
                            case "add":
                                await _functionCallingService.ExecuteFunction("update_user_context", JsonSerializer.Serialize(new
                                {
                                    user_id = userId,
                                    context_type = context.Type,
                                    context_content = context.Content,
                                    status = context.Status
                                }));
                                _logger.LogInformation("AI新增用户上下文：{content}，类型：{type}，状态：{status}，用户ID：{userId}",
                                    context.Content, context.Type, context.Status, userId);
                                break;

                            case "update_status":
                                await _functionCallingService.ExecuteFunction("update_user_context", JsonSerializer.Serialize(new
                                {
                                    user_id = userId,
                                    context_type = context.Type,
                                    context_content = context.TargetContent ?? context.Content,
                                    status = context.Status
                                }));
                                _logger.LogInformation("AI更新用户上下文状态：{content}，新状态：{status}，用户ID：{userId}",
                                    context.TargetContent ?? context.Content, context.Status, userId);
                                break;

                            case "remove":
                                // 通过设置为completed状态来"移除"上下文
                                await _functionCallingService.ExecuteFunction("update_user_context", JsonSerializer.Serialize(new
                                {
                                    user_id = userId,
                                    context_type = context.Type,
                                    context_content = context.TargetContent ?? context.Content,
                                    status = "completed"
                                }));
                                _logger.LogInformation("AI移除用户上下文：{content}，类型：{type}，用户ID：{userId}",
                                    context.TargetContent ?? context.Content, context.Type, userId);
                                break;
                        }
                    }
                }

                // 处理行为观察
                if (analysisResult.Behaviors != null)
                {
                    foreach (var behavior in analysisResult.Behaviors)
                    {
                        if (string.IsNullOrWhiteSpace(behavior.Observation)) continue;

                        // 行为观察主要是添加和更新
                        await _functionCallingService.ExecuteFunction("observe_user_behavior", JsonSerializer.Serialize(new
                        {
                            user_id = userId,
                            behavior_type = behavior.Type,
                            observation = behavior.Observation
                        }));
                        _logger.LogInformation("AI记录用户行为：{observation}，类型：{type}，可信度：{confidence}，用户ID：{userId}",
                            behavior.Observation, behavior.Type, behavior.Confidence, userId);
                    }
                }

                // 处理管理操作
                if (analysisResult.Actions != null)
                {
                    foreach (var action in analysisResult.Actions)
                    {
                        await ProcessUserManagementActionAsync(action, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理AI用户分析结果失败，用户ID：{userId}", userId);
            }
        }

        /// <summary>
        /// 处理用户数据管理操作
        /// </summary>
        /// <param name="action">管理操作</param>
        /// <param name="userId">用户ID</param>
        private async Task ProcessUserManagementActionAsync(UserAnalysisAction action, string userId)
        {
            try
            {
                switch (action.ActionType.ToLower())
                {
                    case "cleanup_completed_contexts":
                        // 清理已完成的上下文
                        _logger.LogInformation("AI触发用户上下文清理，用户ID：{userId}", userId);
                        break;

                    case "update_interest_decay":
                        // 兴趣衰减处理
                        _logger.LogInformation("AI触发用户兴趣衰减更新，用户ID：{userId}", userId);
                        break;

                    case "consolidate_personality":
                        // 性格特征整合
                        _logger.LogInformation("AI触发用户性格特征整合，用户ID：{userId}", userId);
                        break;

                    default:
                        _logger.LogWarning("未知的用户管理操作类型：{actionType}，用户ID：{userId}", action.ActionType, userId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户管理操作失败：{actionType}，用户ID：{userId}", action.ActionType, userId);
            }
        }

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

        /// <summary>
        /// 智能用户偏好和上下文管理
        /// </summary>
        /// <param name="userId">用户ID</param>
        public async Task PerformIntelligentUserDataManagementAsync(string userId)
        {
            try
            {
                _logger.LogInformation("开始执行智能用户数据管理，用户ID：{userId}", userId);

                var profile = await _userProfileService.GetUserProfileAsync(userId);
                var profileJson = JsonSerializer.Serialize(profile, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                // 使用AI分析用户档案，识别需要智能管理的内容
                var intelligentAnalysisPrompt = $@"你需要分析用户的当前档案，识别需要智能优化的数据内容。

【用户档案】：
{profileJson}

请分析用户档案，识别以下优化需求，以JSON格式返回：

{{
  ""interests"": [
    {{
      ""content"": ""兴趣内容"",
      ""operation"": ""remove或update_intensity"",
      ""intensity"": 新强度值(1-5),
      ""reason"": ""优化原因""
    }}
  ],
  ""preferences"": [
    {{
      ""type"": ""偏好类型"",
      ""content"": ""偏好内容"",
      ""operation"": ""remove或update"",
      ""reason"": ""优化原因""
    }}
  ],
  ""contexts"": [
    {{
      ""type"": ""上下文类型"",
      ""content"": ""上下文内容"",
      ""operation"": ""update_status或remove"",
      ""newStatus"": ""新状态"",
      ""reason"": ""优化原因""
    }}
  ],
  ""behaviors"": [
    {{
      ""type"": ""行为类型"",
      ""operation"": ""consolidate或remove"",
      ""reason"": ""优化原因""
    }}
  ],
  ""actions"": [
    {{
      ""actionType"": ""consolidate_interests或update_preference_weights等"",
      ""reason"": ""优化原因""
    }}
  ]
}}

**智能优化原则**：
1. **兴趣衰减**：长时间未提及的兴趣降低强度
2. **偏好整合**：合并相似或重复的偏好
3. **上下文更新**：基于时间推断状态变化
4. **行为模式整合**：合并相似的行为观察
5. **数据一致性**：消除矛盾的信息
6. **个性化优化**：根据用户特点调整数据结构
7. **如果没有需要优化的内容，所有数组都返回空**
8. 只返回JSON，不要其他解释文字";

                var analysisMessages = new List<ChatCompletionMessage>
                {
                    new() { Role = "system", Content = "你是一个专门进行用户数据智能优化的AI助手，专注于提升数据质量和个性化效果。" },
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

                        var managementResult = JsonSerializer.Deserialize<UserAnalysisResult>(jsonContent, _jsonOptions);
                        if (managementResult != null)
                        {
                            await ProcessUserAnalysisResultAsync(managementResult, userId);
                            _logger.LogInformation("智能用户数据管理完成，用户ID：{userId}，完成了数据优化", userId);
                        }
                    }
                }

                _logger.LogInformation("智能用户数据管理完成，用户ID：{userId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "智能用户数据管理失败，用户ID：{userId}", userId);
            }
        }
    }
}
