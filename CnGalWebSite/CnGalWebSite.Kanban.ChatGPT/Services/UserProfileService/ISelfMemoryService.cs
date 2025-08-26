using CnGalWebSite.Kanban.ChatGPT.Models.UserProfile;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService
{
    /// <summary>
    /// 看板娘自我记忆服务接口
    /// </summary>
    public interface ISelfMemoryService
    {
        /// <summary>
        /// 获取自我记忆
        /// </summary>
        /// <param name="memoryId">记忆ID，默认为"global"</param>
        /// <returns>自我记忆模型</returns>
        Task<KanbanSelfMemoryModel> GetSelfMemoryAsync(string memoryId = "global");

        /// <summary>
        /// 记录自我状态
        /// </summary>
        /// <param name="state">状态描述</param>
        /// <param name="stateType">状态类型</param>
        /// <param name="durationMinutes">持续时间（分钟）</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>记录结果</returns>
        Task<bool> RecordSelfStateAsync(string state, string stateType, int durationMinutes = 0, string memoryId = "global");

        /// <summary>
        /// 记录自我特征
        /// </summary>
        /// <param name="trait">特征描述</param>
        /// <param name="traitType">特征类型</param>
        /// <param name="intensity">强度</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>记录结果</returns>
        Task<bool> RecordSelfTraitAsync(string trait, string traitType, int intensity = 3, string memoryId = "global");

        /// <summary>
        /// 创建承诺
        /// </summary>
        /// <param name="commitment">承诺内容</param>
        /// <param name="commitmentType">承诺类型</param>
        /// <param name="expectedTime">预期时间</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>记录结果</returns>
        Task<bool> MakeCommitmentAsync(string commitment, string commitmentType, DateTime? expectedTime = null, string memoryId = "global");

        /// <summary>
        /// 更新承诺状态
        /// </summary>
        /// <param name="commitment">承诺内容</param>
        /// <param name="status">新状态</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateCommitmentStatusAsync(string commitment, string status, string memoryId = "global");

        /// <summary>
        /// 记录对话记忆
        /// </summary>
        /// <param name="content">对话内容</param>
        /// <param name="memoryType">记忆类型</param>
        /// <param name="relatedUserId">相关用户ID</param>
        /// <param name="importance">重要程度</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>记录结果</returns>
        Task<bool> RememberConversationAsync(string content, string memoryType, string? relatedUserId = null, int importance = 3, string memoryId = "global");

        /// <summary>
        /// 获取格式化的自我记忆信息
        /// </summary>
        /// <param name="memoryType">记忆类型</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>格式化的记忆信息</returns>
        Task<string> GetFormattedMemoryAsync(string memoryType, string memoryId = "global");

        /// <summary>
        /// 移除自我状态
        /// </summary>
        /// <param name="state">状态描述</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>移除结果</returns>
        Task<bool> RemoveSelfStateAsync(string state, string memoryId = "global");

        /// <summary>
        /// 移除自我特征
        /// </summary>
        /// <param name="trait">特征描述</param>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>移除结果</returns>
        Task<bool> RemoveSelfTraitAsync(string trait, string memoryId = "global");

        /// <summary>
        /// 清理过期状态
        /// </summary>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>清理结果</returns>
        Task<bool> CleanupExpiredStatesAsync(string memoryId = "global");

        /// <summary>
        /// 清理已完成的承诺
        /// </summary>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>清理结果</returns>
        Task<bool> CleanupCompletedCommitmentsAsync(string memoryId = "global");

        /// <summary>
        /// 数据生命周期管理：自动清理和状态转换
        /// </summary>
        /// <param name="memoryId">记忆ID</param>
        /// <returns>管理结果</returns>
        Task<bool> PerformLifecycleManagementAsync(string memoryId = "global");
    }
}
