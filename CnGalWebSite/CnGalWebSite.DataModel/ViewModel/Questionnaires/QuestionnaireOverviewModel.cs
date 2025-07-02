using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;

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

        public int? MinCompletionTimeSeconds { get; set; }

        public int? MaxCompletionTimeSeconds { get; set; }

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

    /// <summary>
    /// 用户回答列表模型
    /// </summary>
    public class UserResponseListModel
    {
        public long QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; }
        public List<UserResponseSummaryModel> Responses { get; set; } = new List<UserResponseSummaryModel>();
    }

    /// <summary>
    /// 用户回答摘要模型
    /// </summary>
    public class UserResponseSummaryModel
    {
        public long ResponseId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserDisplayName { get; set; }
        public DateTime SubmitTime { get; set; }
        public bool IsCompleted { get; set; }
        public int? CompletionTimeSeconds { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public int AnsweredQuestionCount { get; set; }
        public int TotalQuestionCount { get; set; }
        public int VisibleQuestionCount { get; set; }
        public double CompletionPercentage => VisibleQuestionCount > 0 ? (double)AnsweredQuestionCount / VisibleQuestionCount * 100 : 0;
    }

    /// <summary>
    /// 用户回答详情模型
    /// </summary>
    public class UserResponseDetailModel
    {
        public long ResponseId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserDisplayName { get; set; }
        public DateTime SubmitTime { get; set; }
        public bool IsCompleted { get; set; }
        public int? CompletionTimeSeconds { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public List<UserQuestionAnswerModel> Answers { get; set; } = new List<UserQuestionAnswerModel>();
    }

    /// <summary>
    /// 用户题目回答模型
    /// </summary>
    public class UserQuestionAnswerModel
    {
        public long QuestionId { get; set; }
        public string QuestionTitle { get; set; }
        public QuestionType QuestionType { get; set; }
        public string TextAnswer { get; set; }
        public decimal? NumericAnswer { get; set; }
        public List<string> SelectedOptions { get; set; } = new List<string>();
        public List<string> SortedOptions { get; set; } = new List<string>();
        public DateTime ResponseTime { get; set; }
        /// <summary>
        /// 题目用时（秒）
        /// </summary>
        public int? QuestionDurationSeconds { get; set; }
    }

    /// <summary>
    /// 题目分析模型
    /// </summary>
    public class QuestionAnalysisModel
    {
        public long QuestionnaireId { get; set; }
        public string QuestionnaireName { get; set; }
        public List<QuestionStatisticsModel> Questions { get; set; } = new List<QuestionStatisticsModel>();
    }

    /// <summary>
    /// 单个题目统计模型
    /// </summary>
    public class QuestionStatisticsModel
    {
        public long QuestionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public int TotalResponses { get; set; }
        public int ValidResponses { get; set; }
        public double ResponseRate => TotalResponses > 0 ? (double)ValidResponses / TotalResponses * 100 : 0;

        // 单选/多选题统计
        public List<OptionStatisticsModel> OptionStatistics { get; set; } = new List<OptionStatisticsModel>();

        // 主观题统计
        public List<string> TextAnswers { get; set; } = new List<string>();

        // 数值题统计
        public decimal? AverageNumericValue { get; set; }
        public decimal? MinNumericValue { get; set; }
        public decimal? MaxNumericValue { get; set; }
        public List<decimal> NumericValues { get; set; } = new List<decimal>();

        // 排序题统计
        public List<RankingStatisticsModel> RankingStatistics { get; set; } = new List<RankingStatisticsModel>();

        // 用时统计
        public double? AverageQuestionDurationSeconds { get; set; }
        public int? MinQuestionDurationSeconds { get; set; }
        public int? MaxQuestionDurationSeconds { get; set; }
        public List<int> QuestionDurations { get; set; } = new List<int>();
    }

    /// <summary>
    /// 选项统计模型
    /// </summary>
    public class OptionStatisticsModel
    {
        public long OptionId { get; set; }
        public string OptionText { get; set; }
        public string OptionValue { get; set; }
        public int SelectionCount { get; set; }
        public double SelectionPercentage { get; set; }
    }

    /// <summary>
    /// 排序统计模型
    /// </summary>
    public class RankingStatisticsModel
    {
        public long OptionId { get; set; }
        public string OptionText { get; set; }
        public double AverageRank { get; set; }
        public List<int> RankPositions { get; set; } = new List<int>(); // 每次被排在第几位
        public Dictionary<int, int> RankCounts { get; set; } = new Dictionary<int, int>(); // 各排名的次数
    }
}
