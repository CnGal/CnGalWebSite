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

        private async Task SaveMemoryAsync(KanbanSelfMemoryModel memory)
        {
            var cacheKey = SELF_MEMORY_CACHE_PREFIX + memory.MemoryId;
            await _persistentStorage.SaveAsync(cacheKey, memory);
        }
    }
}
