using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Questionnaires
{
    public class QuestionnaireViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public string MainPicture { get; set; }

        public string Thumbnail { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastEditTime { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsActive { get; set; }

        public bool AllowMultipleSubmissions { get; set; }

        public bool RequireLogin { get; set; }

        public string ThankYouMessage { get; set; }

        public int ResponseCount { get; set; }

        /// <summary>
        /// 是否已经提交过
        /// </summary>
        public bool HasSubmitted { get; set; }

        /// <summary>
        /// 问卷题目
        /// </summary>
        public List<QuestionnaireQuestionViewModel> Questions { get; set; } = new List<QuestionnaireQuestionViewModel>();

        /// <summary>
        /// 当前用户的回答（编辑模式使用）
        /// </summary>
        public QuestionnaireResponseViewModel UserResponse { get; set; }
    }

    public class QuestionnaireQuestionViewModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public QuestionType QuestionType { get; set; }

        public bool IsRequired { get; set; }

        public int SortOrder { get; set; }

        public int? MinSelectionCount { get; set; }

        public int? MaxSelectionCount { get; set; }

        public int? MaxTextLength { get; set; }

        public string TextPlaceholder { get; set; }

        public List<QuestionOptionViewModel> Options { get; set; } = new List<QuestionOptionViewModel>();

        public List<QuestionDisplayConditionViewModel> DisplayConditions { get; set; } = new List<QuestionDisplayConditionViewModel>();

        /// <summary>
        /// 当前用户对此题的回答
        /// </summary>
        public QuestionResponseViewModel UserResponse { get; set; }
    }

    public class QuestionOptionViewModel
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; }

        public string Image { get; set; }

        /// <summary>
        /// 选择该选项的人数（统计模式）
        /// </summary>
        public int SelectionCount { get; set; }
    }

    public class QuestionDisplayConditionViewModel
    {
        public long Id { get; set; }

        public ConditionType ConditionType { get; set; }

        public LogicalOperator LogicalOperator { get; set; }

        public long TriggerQuestionId { get; set; }

        public long? TriggerOptionId { get; set; }

        public string ExpectedValue { get; set; }

        public int ConditionGroup { get; set; }
    }

    public class QuestionnaireResponseViewModel
    {
        public long Id { get; set; }

        public DateTime SubmitTime { get; set; }

        public bool IsCompleted { get; set; }

        public int? CompletionTimeSeconds { get; set; }

        public string ApplicationUserId { get; set; }

        public List<QuestionResponseViewModel> QuestionResponses { get; set; } = new List<QuestionResponseViewModel>();
    }

    public class QuestionResponseViewModel
    {
        public long Id { get; set; }

        public string TextAnswer { get; set; }

        public List<long> SelectedOptionIds { get; set; } = new List<long>();

        public List<long> SortedOptionIds { get; set; } = new List<long>();

        public decimal? NumericAnswer { get; set; }

        public DateTime ResponseTime { get; set; }

        public long QuestionId { get; set; }
    }
}
