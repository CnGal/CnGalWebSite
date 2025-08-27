using CnGalWebSite.Kanban.ChatGPT.Models.UserProfile;
using CnGalWebSite.Kanban.ChatGPT.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService
{
    /// <summary>
    /// 用户个性化设定服务实现
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly IPersistentStorage _persistentStorage;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISelfMemoryService _selfMemoryService;

        // 缓存前缀
        private const string USER_PROFILE_CACHE_PREFIX = "user_profile_";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public UserProfileService(IPersistentStorage persistentStorage,
            ILogger<UserProfileService> logger, IConfiguration configuration, ISelfMemoryService selfMemoryService)
        {
            _persistentStorage = persistentStorage;
            _logger = logger;
            _configuration = configuration;
            _selfMemoryService = selfMemoryService;
        }

        public async Task<UserProfileModel> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new UserProfileModel { UserId = userId };
            }

            var cacheKey = USER_PROFILE_CACHE_PREFIX + userId;

            // 尝试从持久化存储获取
            var persistentProfile = await _persistentStorage.LoadAsync<UserProfileModel>(cacheKey);
            if (persistentProfile != null)
            {
                _logger.LogInformation("用户设定从持久化存储加载，用户ID：{userId}", userId);
                return persistentProfile;
            }

            // 创建默认配置
            var defaultProfile = new UserProfileModel
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 保存到持久化存储
            await _persistentStorage.SaveAsync(cacheKey, defaultProfile);

            _logger.LogInformation("为用户创建默认设定并持久化，用户ID：{userId}", userId);
            return defaultProfile;
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request)
        {
            try
            {
                var profile = await GetUserProfileAsync(userId);
                var updated = false;

                switch (request.Field.ToLower())
                {
                    case "nickname":
                        profile.Nickname = request.Value;
                        updated = true;
                        break;

                    case "personality":
                        profile.Personality = request.Value;
                        updated = true;
                        break;

                    case "communication_style":
                        profile.CommunicationStyle = request.Value;
                        updated = true;
                        break;

                    case "age_group":
                        profile.AgeGroup = request.Value;
                        updated = true;
                        break;

                    case "interests":
                        updated = UpdateListField(profile.Interests, request.Value, request.Operation);
                        break;

                    case "preferred_game_types":
                        updated = UpdateListField(profile.PreferredGameTypes, request.Value, request.Operation);
                        break;

                    case "special_preferences":
                        if (request.Operation == "remove")
                        {
                            updated = profile.SpecialPreferences.Remove(request.Value);
                        }
                        else
                        {
                            // 期望 value 格式为 "key:value"
                            var parts = request.Value.Split(':', 2);
                            if (parts.Length == 2)
                            {
                                profile.SpecialPreferences[parts[0]] = parts[1];
                                updated = true;
                            }
                        }
                        break;

                    case "preferences_internal":
                        try
                        {
                            var preferences = JsonSerializer.Deserialize<Dictionary<string, List<UserPreferenceItem>>>(request.Value);
                            if (preferences != null)
                            {
                                profile.Preferences = preferences;
                                updated = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "反序列化用户偏好失败");
                        }
                        break;

                    case "behavior_observations_internal":
                        try
                        {
                            var observations = JsonSerializer.Deserialize<List<BehaviorObservation>>(request.Value);
                            if (observations != null)
                            {
                                profile.BehaviorObservations = observations;
                                updated = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "反序列化行为观察失败");
                        }
                        break;

                    case "current_contexts_internal":
                        try
                        {
                            var contexts = JsonSerializer.Deserialize<List<UserContextItem>>(request.Value);
                            if (contexts != null)
                            {
                                profile.CurrentContexts = contexts;
                                updated = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "反序列化用户上下文失败");
                        }
                        break;

                    case "complete_profile_replace":
                        // 【新增】支持完整档案替换，用于AI融合后的数据更新
                        try
                        {
                            var newProfile = JsonSerializer.Deserialize<UserProfileModel>(request.Value);
                            if (newProfile != null)
                            {
                                // 保留原有的创建时间和用户ID
                                newProfile.UserId = profile.UserId;
                                newProfile.CreatedAt = profile.CreatedAt;
                                newProfile.UpdatedAt = DateTime.Now;

                                // 完整替换档案内容
                                profile.Nickname = newProfile.Nickname;
                                profile.Personality = newProfile.Personality;
                                profile.CommunicationStyle = newProfile.CommunicationStyle;
                                profile.AgeGroup = newProfile.AgeGroup;
                                profile.Interests = newProfile.Interests ?? new List<string>();
                                profile.PreferredGameTypes = newProfile.PreferredGameTypes ?? new List<string>();
                                profile.SpecialPreferences = newProfile.SpecialPreferences ?? new Dictionary<string, string>();
                                profile.Preferences = newProfile.Preferences ?? new Dictionary<string, List<UserPreferenceItem>>();
                                profile.BehaviorObservations = newProfile.BehaviorObservations ?? new List<BehaviorObservation>();
                                profile.CurrentContexts = newProfile.CurrentContexts ?? new List<UserContextItem>();

                                updated = true;
                                _logger.LogInformation("AI融合档案替换成功，精简数据量：用户ID：{userId}", userId);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "完整档案替换失败");
                        }
                        break;

                    default:
                        _logger.LogWarning("未知的用户设定字段：{field}", request.Field);
                        return false;
                }

                if (updated)
                {
                    profile.UpdatedAt = DateTime.Now;
                    await SaveUserProfileAsync(profile);
                    _logger.LogInformation("成功更新用户设定，用户ID：{userId}，字段：{field}", userId, request.Field);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户设定失败，用户ID：{userId}", userId);
                return false;
            }
        }

        public async Task<bool> SetUserNicknameAsync(string userId, string nickname)
        {
            return await UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
            {
                Field = "nickname",
                Value = nickname,
                Operation = "set"
            });
        }

        public async Task<bool> SetUserPersonalityAsync(string userId, string personality)
        {
            return await UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
            {
                Field = "personality",
                Value = personality,
                Operation = "set"
            });
        }

        public async Task<bool> AddUserInterestAsync(string userId, string interest)
        {
            return await UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
            {
                Field = "interests",
                Value = interest,
                Operation = "add"
            });
        }

        public async Task<bool> RemoveUserInterestAsync(string userId, string interest)
        {
            return await UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
            {
                Field = "interests",
                Value = interest,
                Operation = "remove"
            });
        }

        public async Task<bool> SetUserCommunicationStyleAsync(string userId, string style)
        {
            return await UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
            {
                Field = "communication_style",
                Value = style,
                Operation = "set"
            });
        }

        public async Task<string> GetPersonalizedSystemMessageAsync(string userId, string baseSystemMessage)
        {
            try
            {
                var userProfile = await GetUserProfileAsync(userId);
                var personalizedMessage = baseSystemMessage;

                // 添加用户个性化信息
                var userInfo = "\n\n【用户信息】";
                var hasUserInfo = false;

                if (!string.IsNullOrWhiteSpace(userProfile.Nickname))
                {
                    userInfo += $"\n用户昵称：{userProfile.Nickname}";
                    hasUserInfo = true;
                }

                if (!string.IsNullOrWhiteSpace(userProfile.Personality))
                {
                    userInfo += $"\n用户性格：{userProfile.Personality}";
                    hasUserInfo = true;
                }

                if (userProfile.Interests.Any())
                {
                    userInfo += $"\n用户兴趣：{string.Join("、", userProfile.Interests)}";
                    hasUserInfo = true;
                }

                if (userProfile.PreferredGameTypes.Any())
                {
                    userInfo += $"\n偏好游戏类型：{string.Join("、", userProfile.PreferredGameTypes)}";
                    hasUserInfo = true;
                }

                if (!string.IsNullOrWhiteSpace(userProfile.CommunicationStyle))
                {
                    userInfo += $"\n沟通风格偏好：{userProfile.CommunicationStyle}";
                    hasUserInfo = true;
                }

                if (userProfile.SpecialPreferences.Any())
                {
                    userInfo += $"\n特殊偏好：{string.Join("、", userProfile.SpecialPreferences.Select(kv => $"{kv.Key}={kv.Value}"))}";
                    hasUserInfo = true;
                }

                // 添加偏好信息
                if (userProfile.Preferences.Any())
                {
                    var positivePrefs = new List<string>();
                    var negativePrefs = new List<string>();

                    foreach (var prefCategory in userProfile.Preferences)
                    {
                        foreach (var pref in prefCategory.Value)
                        {
                            if (pref.IsPositive)
                                positivePrefs.Add(pref.Content);
                            else
                                negativePrefs.Add(pref.Content);
                        }
                    }

                    if (positivePrefs.Any())
                    {
                        userInfo += $"\n喜欢：{string.Join("、", positivePrefs)}";
                        hasUserInfo = true;
                    }

                    if (negativePrefs.Any())
                    {
                        userInfo += $"\n不喜欢：{string.Join("、", negativePrefs)}";
                        hasUserInfo = true;
                    }
                }

                // 添加当前关注的话题
                if (userProfile.CurrentContexts.Any(c => c.Status == "active" || c.Status == "interested"))
                {
                    var activeContexts = userProfile.CurrentContexts
                        .Where(c => c.Status == "active" || c.Status == "interested")
                        .Select(c => c.Content)
                        .ToList();

                    if (activeContexts.Any())
                    {
                        userInfo += $"\n当前关注：{string.Join("、", activeContexts)}";
                        hasUserInfo = true;
                    }
                }

                if (hasUserInfo)
                {
                    personalizedMessage += userInfo;
                }

                // 添加看板娘自我记忆
                var selfMemory = await _selfMemoryService.GetFormattedMemoryAsync("all");
                if (selfMemory != "暂无记录的信息")
                {
                    personalizedMessage += "\n\n【看板娘记忆】\n" + selfMemory;
                }

                return personalizedMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成个性化系统消息失败，用户ID：{userId}", userId);
                return baseSystemMessage;
            }
        }

        private bool UpdateListField(List<string> list, string value, string operation)
        {
            switch (operation.ToLower())
            {
                case "add":
                    if (!list.Contains(value))
                    {
                        list.Add(value);
                        return true;
                    }
                    break;

                case "remove":
                    return list.Remove(value);

                case "set":
                    list.Clear();
                    list.Add(value);
                    return true;
            }

            return false;
        }

        private async Task SaveUserProfileAsync(UserProfileModel profile)
        {
            var cacheKey = USER_PROFILE_CACHE_PREFIX + profile.UserId;

            // 保存到持久化存储
            await _persistentStorage.SaveAsync(cacheKey, profile);
        }
    }
}
