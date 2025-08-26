using System.Collections.Generic;

namespace CnGalWebSite.Kanban.ChatGPT.Models.UserAnalysis
{
    /// <summary>
    /// AI用户分析结果模型
    /// </summary>
    public class UserAnalysisResult
    {
        public string? Nickname { get; set; }
        public List<UserAnalysisInterest>? Interests { get; set; }
        public List<UserAnalysisPersonality>? PersonalityTraits { get; set; }
        public List<UserAnalysisPreference>? Preferences { get; set; }
        public List<UserAnalysisContext>? Contexts { get; set; }
        public List<UserAnalysisBehavior>? Behaviors { get; set; }
        public List<UserAnalysisAction>? Actions { get; set; }
    }

    /// <summary>
    /// 用户兴趣分析结果
    /// </summary>
    public class UserAnalysisInterest
    {
        public string Content { get; set; } = "";
        public string Operation { get; set; } = "add"; // add, remove, update_intensity
        public int? Intensity { get; set; } // 1-5, 兴趣强度
    }

    /// <summary>
    /// 用户性格分析结果
    /// </summary>
    public class UserAnalysisPersonality
    {
        public string Content { get; set; } = "";
        public string Operation { get; set; } = "add"; // add, update, remove
        public string? TargetContent { get; set; } // 用于更新时指定目标
    }

    /// <summary>
    /// 用户偏好分析结果
    /// </summary>
    public class UserAnalysisPreference
    {
        public string Type { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsPositive { get; set; }
        public string Operation { get; set; } = "add"; // add, update, remove
        public string? TargetContent { get; set; } // 用于更新时指定目标
    }

    /// <summary>
    /// 用户上下文分析结果
    /// </summary>
    public class UserAnalysisContext
    {
        public string Type { get; set; } = "";
        public string Content { get; set; } = "";
        public string Status { get; set; } = "";
        public string Operation { get; set; } = "add"; // add, update_status, remove
        public string? TargetContent { get; set; } // 用于更新时指定目标
    }

    /// <summary>
    /// 用户行为分析结果
    /// </summary>
    public class UserAnalysisBehavior
    {
        public string Type { get; set; } = "";
        public string Observation { get; set; } = "";
        public string Operation { get; set; } = "add"; // add, update, remove
        public int Confidence { get; set; } = 3; // 1-5, 观察的可信度
    }

    /// <summary>
    /// 用户数据管理操作
    /// </summary>
    public class UserAnalysisAction
    {
        public string ActionType { get; set; } = ""; // cleanup_expired, update_context_status, etc.
        public string? Target { get; set; }
        public string? Parameters { get; set; }
    }
}
