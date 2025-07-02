using System;

namespace CnGalWebSite.DataModel.ViewModel.Questionnaires
{
    /// <summary>
    /// 问卷卡片视图模型（用于列表显示）
    /// </summary>
    public class QuestionnaireCardViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string Thumbnail { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsActive { get; set; }

        public int ResponseCount { get; set; }

        public int Priority { get; set; }

        /// <summary>
        /// 问卷状态
        /// </summary>
        public QuestionnaireStatus Status { get; set; }

        /// <summary>
        /// 是否已经参与
        /// </summary>
        public bool HasParticipated { get; set; }

        /// <summary>
        /// 是否允许修改答案
        /// </summary>
        public bool AllowMultipleSubmissions { get; set; }

        /// <summary>
        /// 题目数量
        /// </summary>
        public int QuestionCount { get; set; }

        /// <summary>
        /// 预计用时（分钟）
        /// </summary>
        public int EstimatedTimeMinutes { get; set; }

        /// <summary>
        /// 是否需要登录
        /// </summary>
        public bool RequireLogin { get; set; }
    }

    /// <summary>
    /// 问卷状态
    /// </summary>
    public enum QuestionnaireStatus
    {
        NotStarted,   // 未开始
        Active,       // 进行中
        Ended,        // 已结束
        Inactive      // 未激活
    }
}
