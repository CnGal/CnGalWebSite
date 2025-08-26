using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.SelfAnalysisService
{
    /// <summary>
    /// 看板娘自我分析服务接口
    /// </summary>
    public interface ISelfAnalysisService
    {
        /// <summary>
        /// 使用AI分析看板娘的回复并自动记录相关信息
        /// </summary>
        /// <param name="reply">看板娘的回复内容</param>
        /// <returns></returns>
        Task AnalyzeAndRecordSelfInfoAsync(string reply);

        /// <summary>
        /// 执行定期数据生命周期管理
        /// </summary>
        /// <returns></returns>
        Task PerformPeriodicLifecycleManagementAsync();

        /// <summary>
        /// 智能状态转换和清理
        /// </summary>
        /// <returns></returns>
        Task PerformIntelligentStateManagementAsync();
    }
}
