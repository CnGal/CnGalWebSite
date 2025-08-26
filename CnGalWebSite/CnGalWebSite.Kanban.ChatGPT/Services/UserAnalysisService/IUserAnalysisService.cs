using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.UserAnalysisService
{
    /// <summary>
    /// 用户AI分析服务接口
    /// </summary>
    public interface IUserAnalysisService
    {
        /// <summary>
        /// 自动分析用户消息并记录用户信息
        /// </summary>
        /// <param name="messages">对话消息列表</param>
        /// <returns></returns>
        Task AnalyzeAndRecordUserInfoAsync(List<ChatCompletionMessage> messages);

        /// <summary>
        /// 从消息中提取用户ID
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>用户ID</returns>
        string ExtractUserIdFromMessage(string content);

        /// <summary>
        /// 执行用户数据生命周期管理
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task PerformUserLifecycleManagementAsync(string userId);

        /// <summary>
        /// 批量执行所有用户的数据生命周期管理
        /// </summary>
        /// <returns></returns>
        Task PerformBatchUserLifecycleManagementAsync();

        /// <summary>
        /// 智能用户偏好和上下文管理
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task PerformIntelligentUserDataManagementAsync(string userId);
    }
}
