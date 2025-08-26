using System.Collections.Generic;

namespace CnGalWebSite.Kanban.ChatGPT.Models.SelfAnalysis
{
    /// <summary>
    /// 看板娘自我分析结果模型
    /// </summary>
    public class SelfAnalysisResult
    {
        public List<SelfAnalysisState>? States { get; set; }
        public List<SelfAnalysisTrait>? Traits { get; set; }
        public List<SelfAnalysisCommitment>? Commitments { get; set; }
        public List<SelfAnalysisAction>? Actions { get; set; }
    }

    /// <summary>
    /// 看板娘状态分析结果
    /// </summary>
    public class SelfAnalysisState
    {
        public string Content { get; set; } = "";
        public string Type { get; set; } = "";
        public int Duration { get; set; }
        public string Operation { get; set; } = "add"; // add, update, remove
        public string? TargetContent { get; set; } // 用于更新或删除时指定目标
    }

    /// <summary>
    /// 看板娘特征分析结果
    /// </summary>
    public class SelfAnalysisTrait
    {
        public string Content { get; set; } = "";
        public string Type { get; set; } = "";
        public int Intensity { get; set; }
        public string Operation { get; set; } = "add"; // add, update, remove
        public string? TargetContent { get; set; } // 用于更新或删除时指定目标
    }

    /// <summary>
    /// 看板娘承诺分析结果
    /// </summary>
    public class SelfAnalysisCommitment
    {
        public string Content { get; set; } = "";
        public string Type { get; set; } = "";
        public string? ExpectedTime { get; set; }
        public string Operation { get; set; } = "add"; // add, update, complete, cancel
        public string? TargetContent { get; set; } // 用于更新或完成时指定目标承诺
        public string? NewStatus { get; set; } // fulfilled, cancelled, pending
    }

    /// <summary>
    /// 看板娘自我管理操作
    /// </summary>
    public class SelfAnalysisAction
    {
        public string ActionType { get; set; } = ""; // cleanup_expired, update_status, etc.
        public string? Target { get; set; }
        public string? Parameters { get; set; }
    }
}
