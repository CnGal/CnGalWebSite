using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CnGalWebSite.Kanban.ChatGPT.Models.UserProfile
{
    /// <summary>
    /// 用户个性化设定模型
    /// </summary>
    public class UserProfileModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户昵称/称呼偏好
        /// </summary>
        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        /// <summary>
        /// 用户性格描述
        /// </summary>
        [JsonPropertyName("personality")]
        public string? Personality { get; set; }

        /// <summary>
        /// 用户兴趣爱好
        /// </summary>
        [JsonPropertyName("interests")]
        public List<string> Interests { get; set; } = [];

        /// <summary>
        /// 用户偏好的游戏类型
        /// </summary>
        [JsonPropertyName("preferred_game_types")]
        public List<string> PreferredGameTypes { get; set; } = [];

        /// <summary>
        /// 用户的沟通风格偏好
        /// </summary>
        [JsonPropertyName("communication_style")]
        public string? CommunicationStyle { get; set; }

        /// <summary>
        /// 用户的年龄段（可选）
        /// </summary>
        [JsonPropertyName("age_group")]
        public string? AgeGroup { get; set; }

        /// <summary>
        /// 用户的特殊偏好设定
        /// </summary>
        [JsonPropertyName("special_preferences")]
        public Dictionary<string, string> SpecialPreferences { get; set; } = [];

        /// <summary>
        /// 用户偏好记录（喜欢/不喜欢的事物）
        /// </summary>
        [JsonPropertyName("preferences")]
        public Dictionary<string, List<UserPreferenceItem>> Preferences { get; set; } = [];

        /// <summary>
        /// 行为观察记录
        /// </summary>
        [JsonPropertyName("behavior_observations")]
        public List<BehaviorObservation> BehaviorObservations { get; set; } = [];

        /// <summary>
        /// 当前上下文信息
        /// </summary>
        [JsonPropertyName("current_contexts")]
        public List<UserContextItem> CurrentContexts { get; set; } = [];

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }



    /// <summary>
    /// 更新用户设定的请求模型
    /// </summary>
    public class UpdateUserProfileRequest
    {
        /// <summary>
        /// 要更新的字段名
        /// </summary>
        [Required]
        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// 新的值
        /// </summary>
        [Required]
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型：set(设置), add(添加到列表), remove(从列表移除)
        /// </summary>
        [JsonPropertyName("operation")]
        public string Operation { get; set; } = "set";
    }

    /// <summary>
    /// 用户偏好项
    /// </summary>
    public class UserPreferenceItem
    {
        /// <summary>
        /// 偏好内容
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 是否为正面偏好（喜欢=true，不喜欢=false）
        /// </summary>
        [JsonPropertyName("is_positive")]
        public bool IsPositive { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        [JsonPropertyName("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 偏好强度（1-5，可选）
        /// </summary>
        [JsonPropertyName("intensity")]
        public int Intensity { get; set; } = 3;
    }

    /// <summary>
    /// 行为观察记录
    /// </summary>
    public class BehaviorObservation
    {
        /// <summary>
        /// 行为类型
        /// </summary>
        [JsonPropertyName("behavior_type")]
        public string BehaviorType { get; set; } = string.Empty;

        /// <summary>
        /// 观察内容
        /// </summary>
        [JsonPropertyName("observation")]
        public string Observation { get; set; } = string.Empty;

        /// <summary>
        /// 观察时间
        /// </summary>
        [JsonPropertyName("observed_at")]
        public DateTime ObservedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 置信度（1-5）
        /// </summary>
        [JsonPropertyName("confidence")]
        public int Confidence { get; set; } = 3;
    }

    /// <summary>
    /// 用户上下文项
    /// </summary>
    public class UserContextItem
    {
        /// <summary>
        /// 上下文类型
        /// </summary>
        [JsonPropertyName("context_type")]
        public string ContextType { get; set; } = string.Empty;

        /// <summary>
        /// 上下文内容
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 看板娘自我记忆模型
    /// </summary>
    public class KanbanSelfMemoryModel
    {
        /// <summary>
        /// 记忆ID（默认为群聊或全局）
        /// </summary>
        public string MemoryId { get; set; } = "global";

        /// <summary>
        /// 个人状态记录（如：累了、开心、在玩游戏等）
        /// </summary>
        [JsonPropertyName("personal_states")]
        public List<SelfStateItem> PersonalStates { get; set; } = [];

        /// <summary>
        /// 个人偏好和特征记录
        /// </summary>
        [JsonPropertyName("personal_traits")]
        public List<SelfTraitItem> PersonalTraits { get; set; } = [];

        /// <summary>
        /// 承诺和计划记录
        /// </summary>
        [JsonPropertyName("commitments")]
        public List<CommitmentItem> Commitments { get; set; } = [];

        /// <summary>
        /// 重要对话片段记录
        /// </summary>
        [JsonPropertyName("conversation_memories")]
        public List<ConversationMemoryItem> ConversationMemories { get; set; } = [];

        /// <summary>
        /// 创建时间
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 自我状态项
    /// </summary>
    public class SelfStateItem
    {
        /// <summary>
        /// 状态描述
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// 状态类型（physical, emotional, activity, etc.）
        /// </summary>
        [JsonPropertyName("state_type")]
        public string StateType { get; set; } = string.Empty;

        /// <summary>
        /// 状态是否仍然有效
        /// </summary>
        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 记录时间
        /// </summary>
        [JsonPropertyName("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 预期持续时间（分钟，0表示未知）
        /// </summary>
        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; } = 0;
    }

    /// <summary>
    /// 自我特征项
    /// </summary>
    public class SelfTraitItem
    {
        /// <summary>
        /// 特征描述
        /// </summary>
        [JsonPropertyName("trait")]
        public string Trait { get; set; } = string.Empty;

        /// <summary>
        /// 特征类型（preference, skill, personality, etc.）
        /// </summary>
        [JsonPropertyName("trait_type")]
        public string TraitType { get; set; } = string.Empty;

        /// <summary>
        /// 强度（1-5）
        /// </summary>
        [JsonPropertyName("intensity")]
        public int Intensity { get; set; } = 3;

        /// <summary>
        /// 记录时间
        /// </summary>
        [JsonPropertyName("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 承诺项
    /// </summary>
    public class CommitmentItem
    {
        /// <summary>
        /// 承诺内容
        /// </summary>
        [JsonPropertyName("commitment")]
        public string Commitment { get; set; } = string.Empty;

        /// <summary>
        /// 承诺类型（promise, plan, intention, etc.）
        /// </summary>
        [JsonPropertyName("commitment_type")]
        public string CommitmentType { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending"; // pending, fulfilled, cancelled

        /// <summary>
        /// 预期时间
        /// </summary>
        [JsonPropertyName("expected_time")]
        public DateTime? ExpectedTime { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        [JsonPropertyName("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 对话记忆项
    /// </summary>
    public class ConversationMemoryItem
    {
        /// <summary>
        /// 记忆内容
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 记忆类型（important_info, funny_moment, shared_experience, etc.）
        /// </summary>
        [JsonPropertyName("memory_type")]
        public string MemoryType { get; set; } = string.Empty;

        /// <summary>
        /// 相关用户ID（如果有）
        /// </summary>
        [JsonPropertyName("related_user_id")]
        public string? RelatedUserId { get; set; }

        /// <summary>
        /// 重要程度（1-5）
        /// </summary>
        [JsonPropertyName("importance")]
        public int Importance { get; set; } = 3;

        /// <summary>
        /// 记录时间
        /// </summary>
        [JsonPropertyName("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}
