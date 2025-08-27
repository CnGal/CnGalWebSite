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
        /// 【优化版】智能融合分析用户信息，输出融合后的完整档案而非增量操作
        /// </summary>
        /// <param name="messages">对话消息列表</param>
        /// <returns></returns>
        Task AnalyzeAndFuseUserInfoAsync(List<ChatCompletionMessage> messages);

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

        // 【已删除】PerformIntelligentUserDataManagementAsync方法 - 没有被引用
    }
}
