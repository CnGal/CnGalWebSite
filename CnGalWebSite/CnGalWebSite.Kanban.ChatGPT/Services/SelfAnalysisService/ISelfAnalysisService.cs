using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.SelfAnalysisService
{
    /// <summary>
    /// 看板娘自我分析服务接口
    /// </summary>
    public interface ISelfAnalysisService
    {
        /// <summary>
        /// 【优化版】智能融合分析看板娘回复，减少冗余数据
        /// </summary>
        /// <param name="reply">看板娘的回复内容</param>
        /// <returns></returns>
        Task AnalyzeAndFuseSelfInfoAsync(string reply);
    }
}
