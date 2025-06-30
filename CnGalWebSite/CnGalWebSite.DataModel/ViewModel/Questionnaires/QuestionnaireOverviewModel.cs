using System;

namespace CnGalWebSite.DataModel.ViewModel.Questionnaires
{
    /// <summary>
    /// 问卷管理概览模型
    /// </summary>
    public class QuestionnaireOverviewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastEditTime { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsActive { get; set; }

        public bool IsHidden { get; set; }

        public int ResponseCount { get; set; }

        public int QuestionCount { get; set; }

        public int Priority { get; set; }

        public QuestionnaireStatus Status { get; set; }
    }

    /// <summary>
    /// 问卷统计模型
    /// </summary>
    public class QuestionnaireStatisticsModel
    {
        public long QuestionnaireId { get; set; }

        public string QuestionnaireName { get; set; }

        public int TotalResponses { get; set; }

        public int CompletedResponses { get; set; }

        public int DraftResponses { get; set; }

        public double CompletionRate { get; set; }

        public double AverageCompletionTimeSeconds { get; set; }

        public DateTime? FirstResponseTime { get; set; }

        public DateTime? LastResponseTime { get; set; }
    }

    /// <summary>
    /// 隐藏问卷模型
    /// </summary>
    public class HiddenQuestionnaireModel
    {
        public long Id { get; set; }

        public bool IsHidden { get; set; }
    }

    /// <summary>
    /// 编辑问卷优先级模型
    /// </summary>
    public class EditQuestionnairePriorityViewModel
    {
        public long[] Ids { get; set; }
    }
}
