using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Questionnaires
{
    public class EditQuestionnaireModel : BaseEditModel
    {
        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        [StringLength(200, ErrorMessage = "显示名称不能超过200个字符")]
        public string DisplayName { get; set; }

        [Display(Name = "问卷描述")]
        [StringLength(2000, ErrorMessage = "描述不能超过2000个字符")]
        public string Description { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }

        [Display(Name = "开始时间")]
        public DateTime? BeginTime { get; set; }

        [Display(Name = "结束时间")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "是否激活")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "允许多次提交")]
        public bool AllowMultipleSubmissions { get; set; } = false;

        [Display(Name = "是否需要登录")]
        public bool RequireLogin { get; set; } = true;

        [Display(Name = "优先级")]
        public int Priority { get; set; } = 0;

        [Display(Name = "感谢信息")]
        [StringLength(1000, ErrorMessage = "感谢信息不能超过1000个字符")]
        public string ThankYouMessage { get; set; } = "感谢您的参与！";

        [Display(Name = "题目")]
        public List<EditQuestionnaireQuestionModel> Questions { get; set; } = new List<EditQuestionnaireQuestionModel>();

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(DisplayName))
            {
                return new Result { Successful = false, Error = "请填写所有必填项目" };
            }

            if (BeginTime.HasValue && EndTime.HasValue && BeginTime > EndTime)
            {
                return new Result { Successful = false, Error = "开始时间必须早于结束时间" };
            }

            if (Questions.Count == 0)
            {
                return new Result { Successful = false, Error = "问卷至少要包含一个题目" };
            }

            // 验证题目
            for (int i = 0; i < Questions.Count; i++)
            {
                var result = Questions[i].Validate();
                if (!result.Successful)
                {
                    return new Result { Successful = false, Error = $"第{i + 1}个题目：{result.Error}" };
                }
            }

            // 验证题目排序唯一性
            var sortOrders = Questions.Select(q => q.SortOrder).ToList();
            if (sortOrders.Count != sortOrders.Distinct().Count())
            {
                return new Result { Successful = false, Error = "题目排序不能重复" };
            }

            return new Result { Successful = true };
        }
    }

    public class EditQuestionnaireQuestionModel
    {
        [Display(Name = "题目ID")]
        public long Id { get; set; }

        [Display(Name = "题目标题")]
        [Required(ErrorMessage = "请填写题目标题")]
        [StringLength(500, ErrorMessage = "题目标题不能超过500个字符")]
        public string Title { get; set; }

        [Display(Name = "题目描述")]
        [StringLength(1000, ErrorMessage = "题目描述不能超过1000个字符")]
        public string Description { get; set; }

        [Display(Name = "题目类型")]
        public QuestionType QuestionType { get; set; }

        [Display(Name = "是否必填")]
        public bool IsRequired { get; set; } = false;

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "最小选择数量")]
        public int? MinSelectionCount { get; set; }

        [Display(Name = "最大选择数量")]
        public int? MaxSelectionCount { get; set; }

        [Display(Name = "文本最大长度")]
        public int? MaxTextLength { get; set; }

        [Display(Name = "文本提示")]
        [StringLength(200, ErrorMessage = "文本提示不能超过200个字符")]
        public string TextPlaceholder { get; set; }

        [Display(Name = "选项")]
        public List<EditQuestionOptionModel> Options { get; set; } = new List<EditQuestionOptionModel>();

        [Display(Name = "显示条件")]
        public List<EditQuestionDisplayConditionModel> DisplayConditions { get; set; } = new List<EditQuestionDisplayConditionModel>();

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return new Result { Successful = false, Error = "题目标题不能为空" };
            }

            // 验证选择题必须有选项
            if ((QuestionType == QuestionType.SingleChoice ||
                 QuestionType == QuestionType.MultipleChoice ||
                 QuestionType == QuestionType.Ranking ||
                 QuestionType == QuestionType.MultipleRanking) &&
                Options.Count == 0)
            {
                return new Result { Successful = false, Error = "选择题和排序题必须设置选项" };
            }

            // 验证多选题的选择数量限制
            if (QuestionType == QuestionType.MultipleChoice || QuestionType == QuestionType.MultipleRanking)
            {
                if (MinSelectionCount.HasValue && MinSelectionCount < 1)
                {
                    return new Result { Successful = false, Error = "最小选择数量必须大于0" };
                }

                if (MaxSelectionCount.HasValue && MaxSelectionCount > Options.Count)
                {
                    return new Result { Successful = false, Error = "最大选择数量不能超过选项总数" };
                }

                if (MinSelectionCount.HasValue && MaxSelectionCount.HasValue && MinSelectionCount > MaxSelectionCount)
                {
                    return new Result { Successful = false, Error = "最小选择数量不能大于最大选择数量" };
                }
            }

            // 验证选项
            for (int i = 0; i < Options.Count; i++)
            {
                var result = Options[i].Validate();
                if (!result.Successful)
                {
                    return new Result { Successful = false, Error = $"选项{i + 1}：{result.Error}" };
                }
            }

            return new Result { Successful = true };
        }
    }

    public class EditQuestionOptionModel
    {
        [Display(Name = "选项ID")]
        public long Id { get; set; }

        [Display(Name = "选项文本")]
        [Required(ErrorMessage = "请填写选项文本")]
        [StringLength(500, ErrorMessage = "选项文本不能超过500个字符")]
        public string Text { get; set; }

        [Display(Name = "选项值")]
        [StringLength(100, ErrorMessage = "选项值不能超过100个字符")]
        public string Value { get; set; }

        [Display(Name = "排序")]
        public int SortOrder { get; set; }

        [Display(Name = "是否启用")]
        public bool IsEnabled { get; set; } = true;

        [Display(Name = "选项图片")]
        public string Image { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return new Result { Successful = false, Error = "选项文本不能为空" };
            }

            return new Result { Successful = true };
        }
    }

    public class EditQuestionDisplayConditionModel
    {
        [Display(Name = "条件ID")]
        public long Id { get; set; }

        [Display(Name = "条件类型")]
        public ConditionType ConditionType { get; set; }

        [Display(Name = "逻辑操作符")]
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;

        [Display(Name = "触发题目ID")]
        public long? TriggerQuestionId { get; set; }

        [Display(Name = "触发选项ID")]
        public long? TriggerOptionId { get; set; }

        [Display(Name = "预期值")]
        [StringLength(500, ErrorMessage = "预期值不能超过500个字符")]
        public string ExpectedValue { get; set; }

        [Display(Name = "条件组")]
        public int ConditionGroup { get; set; } = 1;
    }
}
