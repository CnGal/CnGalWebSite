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
    /// 看板娘自我记忆服务实现
    /// </summary>
    public class SelfMemoryService : ISelfMemoryService
    {
        private readonly IPersistentStorage _persistentStorage;
        private readonly ILogger<SelfMemoryService> _logger;
        private readonly IConfiguration _configuration;

        private const string SELF_MEMORY_CACHE_PREFIX = "kanban_self_memory_";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public SelfMemoryService(IPersistentStorage persistentStorage,
            ILogger<SelfMemoryService> logger, IConfiguration configuration)
        {
            _persistentStorage = persistentStorage;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<KanbanSelfMemoryModel> GetSelfMemoryAsync(string memoryId = "global")
        {
            var cacheKey = SELF_MEMORY_CACHE_PREFIX + memoryId;

            // 尝试从持久化存储获取
            var persistentMemory = await _persistentStorage.LoadAsync<KanbanSelfMemoryModel>(cacheKey);
            if (persistentMemory != null)
            {
                _logger.LogInformation("看板娘自我记忆从持久化存储加载，记忆ID：{memoryId}", memoryId);
                return persistentMemory;
            }

            // 创建新的记忆
            var newMemory = new KanbanSelfMemoryModel
            {
                MemoryId = memoryId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 保存到持久化存储
            await _persistentStorage.SaveAsync(cacheKey, newMemory);

            _logger.LogInformation("创建新的看板娘自我记忆并持久化，记忆ID：{memoryId}", memoryId);
            return newMemory;
        }

        public async Task<bool> RecordSelfStateAsync(string state, string stateType, int durationMinutes = 0, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                // 如果是相同类型的状态，先将之前的设为非活跃
                var existingStates = memory.PersonalStates.Where(s => s.StateType == stateType && s.IsActive).ToList();
                foreach (var existingState in existingStates)
                {
                    existingState.IsActive = false;
                }

                // 添加新状态
                memory.PersonalStates.Add(new SelfStateItem
                {
                    State = state,
                    StateType = stateType,
                    IsActive = true,
                    RecordedAt = DateTime.Now,
                    DurationMinutes = durationMinutes
                });

                // 清理旧状态（保留最近的50个）
                if (memory.PersonalStates.Count > 50)
                {
                    memory.PersonalStates = memory.PersonalStates
                        .OrderByDescending(s => s.RecordedAt)
                        .Take(50)
                        .ToList();
                }

                memory.UpdatedAt = DateTime.Now;
                await SaveMemoryAsync(memory);

                _logger.LogInformation("记录看板娘状态成功，状态：{state}，类型：{stateType}", state, stateType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录看板娘状态失败");
                return false;
            }
        }

        public async Task<bool> RecordSelfTraitAsync(string trait, string traitType, int intensity = 3, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                // 检查是否已存在相似特征
                var existingTrait = memory.PersonalTraits
                    .FirstOrDefault(t => t.Trait.Equals(trait, StringComparison.OrdinalIgnoreCase) && t.TraitType == traitType);

                if (existingTrait != null)
                {
                    existingTrait.Intensity = intensity;
                    existingTrait.RecordedAt = DateTime.Now;
                }
                else
                {
                    memory.PersonalTraits.Add(new SelfTraitItem
                    {
                        Trait = trait,
                        TraitType = traitType,
                        Intensity = intensity,
                        RecordedAt = DateTime.Now
                    });
                }

                memory.UpdatedAt = DateTime.Now;
                await SaveMemoryAsync(memory);

                _logger.LogInformation("记录看板娘特征成功，特征：{trait}，类型：{traitType}", trait, traitType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录看板娘特征失败");
                return false;
            }
        }

        public async Task<bool> MakeCommitmentAsync(string commitment, string commitmentType, DateTime? expectedTime = null, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                memory.Commitments.Add(new CommitmentItem
                {
                    Commitment = commitment,
                    CommitmentType = commitmentType,
                    Status = "pending",
                    ExpectedTime = expectedTime,
                    RecordedAt = DateTime.Now
                });

                // 清理旧承诺（保留最近的30个）
                if (memory.Commitments.Count > 30)
                {
                    memory.Commitments = memory.Commitments
                        .OrderByDescending(c => c.RecordedAt)
                        .Take(30)
                        .ToList();
                }

                memory.UpdatedAt = DateTime.Now;
                await SaveMemoryAsync(memory);

                _logger.LogInformation("记录看板娘承诺成功，承诺：{commitment}", commitment);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录看板娘承诺失败");
                return false;
            }
        }

        public async Task<bool> UpdateCommitmentStatusAsync(string commitment, string status, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                var existingCommitment = memory.Commitments
                    .FirstOrDefault(c => c.Commitment.Contains(commitment, StringComparison.OrdinalIgnoreCase) && c.Status == "pending");

                if (existingCommitment != null)
                {
                    existingCommitment.Status = status;
                    memory.UpdatedAt = DateTime.Now;
                    await SaveMemoryAsync(memory);

                    _logger.LogInformation("更新看板娘承诺状态成功，承诺：{commitment}，状态：{status}", commitment, status);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新看板娘承诺状态失败");
                return false;
            }
        }

        public async Task<bool> RememberConversationAsync(string content, string memoryType, string? relatedUserId = null, int importance = 3, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                memory.ConversationMemories.Add(new ConversationMemoryItem
                {
                    Content = content,
                    MemoryType = memoryType,
                    RelatedUserId = relatedUserId,
                    Importance = importance,
                    RecordedAt = DateTime.Now
                });

                // 清理旧记忆（保留最近的100个）
                if (memory.ConversationMemories.Count > 100)
                {
                    memory.ConversationMemories = memory.ConversationMemories
                        .OrderByDescending(m => m.Importance)
                        .ThenByDescending(m => m.RecordedAt)
                        .Take(100)
                        .ToList();
                }

                memory.UpdatedAt = DateTime.Now;
                await SaveMemoryAsync(memory);

                _logger.LogInformation("记录看板娘对话记忆成功，类型：{memoryType}", memoryType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录看板娘对话记忆失败");
                return false;
            }
        }

        public async Task<string> GetFormattedMemoryAsync(string memoryType, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                return memoryType.ToLower() switch
                {
                    "states" => FormatStates(memory.PersonalStates),
                    "traits" => FormatTraits(memory.PersonalTraits),
                    "commitments" => FormatCommitments(memory.Commitments),
                    "conversations" => FormatConversations(memory.ConversationMemories),
                    "all" => FormatAllMemory(memory),
                    _ => JsonSerializer.Serialize(memory, _jsonOptions)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取格式化记忆失败");
                return "获取记忆失败";
            }
        }

        private string FormatStates(List<SelfStateItem> states)
        {
            var activeStates = states.Where(s => s.IsActive).ToList();
            if (!activeStates.Any())
                return "当前没有记录的状态";

            var statesByType = activeStates.GroupBy(s => s.StateType);
            var formattedStates = new List<string>();

            foreach (var group in statesByType)
            {
                var stateList = string.Join("、", group.Select(s => s.State));
                formattedStates.Add($"{group.Key}：{stateList}");
            }

            return $"当前状态：{string.Join("；", formattedStates)}";
        }

        private string FormatTraits(List<SelfTraitItem> traits)
        {
            if (!traits.Any())
                return "当前没有记录的特征";

            var traitsByType = traits.GroupBy(t => t.TraitType);
            var formattedTraits = new List<string>();

            foreach (var group in traitsByType)
            {
                var traitList = string.Join("、", group.Select(t => t.Trait));
                formattedTraits.Add($"{group.Key}：{traitList}");
            }

            return $"个人特征：{string.Join("；", formattedTraits)}";
        }

        private string FormatCommitments(List<CommitmentItem> commitments)
        {
            var pendingCommitments = commitments.Where(c => c.Status == "pending").ToList();
            if (!pendingCommitments.Any())
                return "当前没有待完成的承诺";

            var commitmentList = pendingCommitments.Select(c =>
                c.ExpectedTime.HasValue ?
                $"{c.Commitment}（预期：{c.ExpectedTime.Value:MM-dd HH:mm}）" :
                c.Commitment);

            return $"待完成的承诺：{string.Join("；", commitmentList)}";
        }

        private string FormatConversations(List<ConversationMemoryItem> conversations)
        {
            var recentMemories = conversations.OrderByDescending(c => c.Importance)
                .ThenByDescending(c => c.RecordedAt)
                .Take(10)
                .ToList();

            if (!recentMemories.Any())
                return "当前没有重要的对话记忆";

            var memoryList = recentMemories.Select(m => $"{m.MemoryType}：{m.Content}");
            return $"重要记忆：{string.Join("；", memoryList)}";
        }

        private string FormatAllMemory(KanbanSelfMemoryModel memory)
        {
            var parts = new List<string>();

            var statesText = FormatStates(memory.PersonalStates);
            if (statesText != "当前没有记录的状态")
                parts.Add(statesText);

            var traitsText = FormatTraits(memory.PersonalTraits);
            if (traitsText != "当前没有记录的特征")
                parts.Add(traitsText);

            var commitmentsText = FormatCommitments(memory.Commitments);
            if (commitmentsText != "当前没有待完成的承诺")
                parts.Add(commitmentsText);

            return parts.Any() ? string.Join("\n", parts) : "暂无记录的信息";
        }

        public async Task<bool> RemoveSelfStateAsync(string state, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                var stateToRemove = memory.PersonalStates
                    .FirstOrDefault(s => s.State.Contains(state, StringComparison.OrdinalIgnoreCase));

                if (stateToRemove != null)
                {
                    memory.PersonalStates.Remove(stateToRemove);
                    memory.UpdatedAt = DateTime.Now;
                    await SaveMemoryAsync(memory);

                    _logger.LogInformation("移除看板娘状态成功，状态：{state}", state);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除看板娘状态失败");
                return false;
            }
        }

        public async Task<bool> RemoveSelfTraitAsync(string trait, string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);

                var traitToRemove = memory.PersonalTraits
                    .FirstOrDefault(t => t.Trait.Contains(trait, StringComparison.OrdinalIgnoreCase));

                if (traitToRemove != null)
                {
                    memory.PersonalTraits.Remove(traitToRemove);
                    memory.UpdatedAt = DateTime.Now;
                    await SaveMemoryAsync(memory);

                    _logger.LogInformation("移除看板娘特征成功，特征：{trait}", trait);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除看板娘特征失败");
                return false;
            }
        }

        public async Task<bool> CleanupExpiredStatesAsync(string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);
                var now = DateTime.Now;
                var expiredStates = new List<SelfStateItem>();

                foreach (var state in memory.PersonalStates.Where(s => s.IsActive && s.DurationMinutes > 0))
                {
                    var expiryTime = state.RecordedAt.AddMinutes(state.DurationMinutes);
                    if (now > expiryTime)
                    {
                        state.IsActive = false;
                        expiredStates.Add(state);
                    }
                }

                if (expiredStates.Any())
                {
                    memory.UpdatedAt = DateTime.Now;
                    await SaveMemoryAsync(memory);

                    _logger.LogInformation("清理过期状态成功，共清理：{count}个状态", expiredStates.Count);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期状态失败");
                return false;
            }
        }

        public async Task<bool> CleanupCompletedCommitmentsAsync(string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);
                var completedCommitments = memory.Commitments
                    .Where(c => c.Status == "fulfilled" && c.RecordedAt < DateTime.Now.AddDays(-7))
                    .ToList();

                if (completedCommitments.Any())
                {
                    foreach (var commitment in completedCommitments)
                    {
                        memory.Commitments.Remove(commitment);
                    }

                    memory.UpdatedAt = DateTime.Now;
                    await SaveMemoryAsync(memory);

                    _logger.LogInformation("清理已完成承诺成功，共清理：{count}个承诺", completedCommitments.Count);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理已完成承诺失败");
                return false;
            }
        }

        public async Task<bool> PerformLifecycleManagementAsync(string memoryId = "global")
        {
            try
            {
                var memory = await GetSelfMemoryAsync(memoryId);
                var now = DateTime.Now;
                bool hasChanges = false;

                // 1. 自动过期状态
                var expiredCount = 0;
                foreach (var state in memory.PersonalStates.Where(s => s.IsActive && s.DurationMinutes > 0))
                {
                    var expiryTime = state.RecordedAt.AddMinutes(state.DurationMinutes);
                    if (now > expiryTime)
                    {
                        state.IsActive = false;
                        expiredCount++;
                        hasChanges = true;
                    }
                }

                // 2. 清理长期非活跃状态（超过24小时的非活跃状态）
                var oldInactiveStates = memory.PersonalStates
                    .Where(s => !s.IsActive && s.RecordedAt < now.AddHours(-24))
                    .ToList();
                foreach (var oldState in oldInactiveStates)
                {
                    memory.PersonalStates.Remove(oldState);
                    hasChanges = true;
                }

                // 3. 自动过期承诺（超过预期时间7天的待完成承诺）
                var overdueCommitments = memory.Commitments
                    .Where(c => c.Status == "pending" &&
                           c.ExpectedTime.HasValue &&
                           c.ExpectedTime.Value.AddDays(7) < now)
                    .ToList();
                foreach (var commitment in overdueCommitments)
                {
                    commitment.Status = "expired";
                    hasChanges = true;
                }

                // 4. 清理旧的已完成承诺（7天前完成的）
                var oldCompletedCommitments = memory.Commitments
                    .Where(c => (c.Status == "fulfilled" || c.Status == "cancelled") &&
                           c.RecordedAt < now.AddDays(-7))
                    .ToList();
                foreach (var commitment in oldCompletedCommitments)
                {
                    memory.Commitments.Remove(commitment);
                    hasChanges = true;
                }

                // 5. 清理低重要度的旧对话记忆（保留高重要度的）
                if (memory.ConversationMemories.Count > 100)
                {
                    var memoriesToKeep = memory.ConversationMemories
                        .Where(m => m.Importance >= 4 || m.RecordedAt > now.AddDays(-3))
                        .OrderByDescending(m => m.Importance)
                        .ThenByDescending(m => m.RecordedAt)
                        .Take(80)
                        .ToList();

                    if (memoriesToKeep.Count < memory.ConversationMemories.Count)
                    {
                        memory.ConversationMemories = memoriesToKeep;
                        hasChanges = true;
                    }
                }

                // 6. 特征强度衰减（长时间未更新的特征降低强度）
                foreach (var trait in memory.PersonalTraits.Where(t => t.RecordedAt < now.AddDays(-30)))
                {
                    if (trait.Intensity > 1)
                    {
                        trait.Intensity = Math.Max(1, trait.Intensity - 1);
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    memory.UpdatedAt = DateTime.Now;
                    await SaveMemoryAsync(memory);

                    _logger.LogInformation("数据生命周期管理完成，已过期状态：{expiredCount}个，清理旧数据，更新特征强度", expiredCount);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据生命周期管理失败");
                return false;
            }
        }

        /// <summary>
        /// 完整替换看板娘记忆
        /// </summary>
        /// <param name="fusedMemory">融合后的完整记忆</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>替换结果</returns>
        public async Task<bool> ReplaceCompleteMemoryAsync(KanbanSelfMemoryModel fusedMemory, string memoryId = "global")
        {
            try
            {
                // 确保记忆ID一致
                fusedMemory.MemoryId = memoryId;
                fusedMemory.UpdatedAt = DateTime.Now;

                // 验证融合后的记忆数据
                ValidateFusedMemory(fusedMemory);

                // 直接替换完整记忆
                await SaveMemoryAsync(fusedMemory);

                _logger.LogInformation("成功完整替换看板娘记忆，记忆ID：{memoryId}。状态数：{states}，特征数：{traits}，承诺数：{commitments}，对话记忆数：{conversations}",
                    memoryId,
                    fusedMemory.PersonalStates?.Count ?? 0,
                    fusedMemory.PersonalTraits?.Count ?? 0,
                    fusedMemory.Commitments?.Count ?? 0,
                    fusedMemory.ConversationMemories?.Count ?? 0);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "完整替换看板娘记忆失败，记忆ID：{memoryId}", memoryId);
                return false;
            }
        }

        /// <summary>
        /// 验证融合后的记忆数据
        /// </summary>
        /// <param name="fusedMemory">融合后的记忆</param>
        private void ValidateFusedMemory(KanbanSelfMemoryModel fusedMemory)
        {
            // 设置创建时间（如果尚未设置）
            if (fusedMemory.CreatedAt == DateTime.MinValue)
            {
                fusedMemory.CreatedAt = DateTime.Now;
            }

            // 验证数据完整性
            foreach (var state in fusedMemory.PersonalStates)
            {
                if (state.RecordedAt == DateTime.MinValue)
                    state.RecordedAt = DateTime.Now;
            }

            foreach (var trait in fusedMemory.PersonalTraits)
            {
                if (trait.RecordedAt == DateTime.MinValue)
                    trait.RecordedAt = DateTime.Now;
                // 确保强度在合理范围内
                trait.Intensity = Math.Max(1, Math.Min(5, trait.Intensity));
            }

            foreach (var commitment in fusedMemory.Commitments)
            {
                if (commitment.RecordedAt == DateTime.MinValue)
                    commitment.RecordedAt = DateTime.Now;
            }

            foreach (var memory in fusedMemory.ConversationMemories)
            {
                if (memory.RecordedAt == DateTime.MinValue)
                    memory.RecordedAt = DateTime.Now;
                // 确保重要度在合理范围内
                memory.Importance = Math.Max(1, Math.Min(5, memory.Importance));
            }

            _logger.LogDebug("融合记忆数据验证完成，记忆ID：{memoryId}", fusedMemory.MemoryId);
        }

        private async Task SaveMemoryAsync(KanbanSelfMemoryModel memory)
        {
            var cacheKey = SELF_MEMORY_CACHE_PREFIX + memory.MemoryId;
            await _persistentStorage.SaveAsync(cacheKey, memory);
        }
    }
}
