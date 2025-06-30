using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    /// <summary>
    /// 问卷
    /// </summary>
    public class Questionnaire
    {
        public long Id { get; set; }

        /// <summary>
        /// 问卷名称（唯一标识）
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [StringLength(200)]
        public string DisplayName { get; set; }

        /// <summary>
        /// 问卷描述
        /// </summary>
        [StringLength(2000)]
        public string Description { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否允许多次提交
        /// </summary>
        public bool AllowMultipleSubmissions { get; set; } = false;

        /// <summary>
        /// 是否需要登录
        /// </summary>
        public bool RequireLogin { get; set; } = true;

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 提交后显示的感谢信息
        /// </summary>
        [StringLength(1000)]
        public string ThankYouMessage { get; set; }

        /// <summary>
        /// 总回答数
        /// </summary>
        public int ResponseCount { get; set; } = 0;

        /// <summary>
        /// 问卷题目
        /// </summary>
        public virtual ICollection<QuestionnaireQuestion> Questions { get; set; } = new List<QuestionnaireQuestion>();

        /// <summary>
        /// 问卷回答
        /// </summary>
        public virtual ICollection<QuestionnaireResponse> Responses { get; set; } = new List<QuestionnaireResponse>();
    }

    /// <summary>
    /// 问卷题目
    /// </summary>
    public class QuestionnaireQuestion
    {
        public long Id { get; set; }

        /// <summary>
        /// 题目标题
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Title { get; set; }

        /// <summary>
        /// 题目描述
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// 题目类型
        /// </summary>
        public QuestionType QuestionType { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 最小选择数量（多选题使用）
        /// </summary>
        public int? MinSelectionCount { get; set; }

        /// <summary>
        /// 最大选择数量（多选题使用）
        /// </summary>
        public int? MaxSelectionCount { get; set; }

        /// <summary>
        /// 文本输入最大长度（主观题使用）
        /// </summary>
        public int? MaxTextLength { get; set; }

        /// <summary>
        /// 文本输入提示
        /// </summary>
        [StringLength(200)]
        public string TextPlaceholder { get; set; }

        /// <summary>
        /// 所属问卷
        /// </summary>
        public long QuestionnaireId { get; set; }
        public virtual Questionnaire Questionnaire { get; set; }

        /// <summary>
        /// 题目选项
        /// </summary>
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();

        /// <summary>
        /// 显示条件（此题目的显示条件）
        /// </summary>
        public virtual ICollection<QuestionDisplayCondition> DisplayConditions { get; set; } = new List<QuestionDisplayCondition>();

        /// <summary>
        /// 触发的显示条件（此题目作为触发条件的目标）
        /// </summary>
        public virtual ICollection<QuestionDisplayCondition> TriggeredConditions { get; set; } = new List<QuestionDisplayCondition>();

        /// <summary>
        /// 题目回答
        /// </summary>
        public virtual ICollection<QuestionResponse> Responses { get; set; } = new List<QuestionResponse>();
    }

    /// <summary>
    /// 题目选项
    /// </summary>
    public class QuestionOption
    {
        public long Id { get; set; }

        /// <summary>
        /// 选项文本
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        /// <summary>
        /// 选项值（用于数据分析）
        /// </summary>
        [StringLength(100)]
        public string Value { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 选项图片（可选）
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 所属题目
        /// </summary>
        public long QuestionId { get; set; }
        public virtual QuestionnaireQuestion Question { get; set; }

        /// <summary>
        /// 作为触发条件的显示条件
        /// </summary>
        public virtual ICollection<QuestionDisplayCondition> TriggeredConditions { get; set; } = new List<QuestionDisplayCondition>();
    }

    /// <summary>
    /// 题目显示条件
    /// </summary>
    public class QuestionDisplayCondition
    {
        public long Id { get; set; }

        /// <summary>
        /// 条件类型
        /// </summary>
        public ConditionType ConditionType { get; set; }

        /// <summary>
        /// 逻辑操作符（与其他条件的关系）
        /// </summary>
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;

        /// <summary>
        /// 触发题目ID
        /// </summary>
        public long TriggerQuestionId { get; set; }
        public virtual QuestionnaireQuestion TriggerQuestion { get; set; }

        /// <summary>
        /// 触发选项ID（可选，用于选择题）
        /// </summary>
        public long? TriggerOptionId { get; set; }
        public virtual QuestionOption TriggerOption { get; set; }

        /// <summary>
        /// 预期值（用于文本匹配等）
        /// </summary>
        [StringLength(500)]
        public string ExpectedValue { get; set; }

        /// <summary>
        /// 被控制显示的题目ID
        /// </summary>
        public long ControlledQuestionId { get; set; }
        public virtual QuestionnaireQuestion ControlledQuestion { get; set; }

        /// <summary>
        /// 条件组（同组条件使用逻辑操作符连接）
        /// </summary>
        public int ConditionGroup { get; set; } = 1;
    }

    /// <summary>
    /// 问卷回答
    /// </summary>
    public class QuestionnaireResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime SubmitTime { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// IP地址
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; }

        /// <summary>
        /// 用户代理
        /// </summary>
        [StringLength(500)]
        public string UserAgent { get; set; }

        /// <summary>
        /// 会话ID（用于匿名用户）
        /// </summary>
        [StringLength(100)]
        public string SessionId { get; set; }

        /// <summary>
        /// 完成用时（秒）
        /// </summary>
        public int? CompletionTimeSeconds { get; set; }

        /// <summary>
        /// 所属问卷
        /// </summary>
        public long QuestionnaireId { get; set; }
        public virtual Questionnaire Questionnaire { get; set; }

        /// <summary>
        /// 回答用户（可选，匿名问卷可为空）
        /// </summary>
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        /// <summary>
        /// 题目回答
        /// </summary>
        public virtual ICollection<QuestionResponse> QuestionResponses { get; set; } = new List<QuestionResponse>();
    }

    /// <summary>
    /// 题目回答
    /// </summary>
    public class QuestionResponse
    {
        public long Id { get; set; }

        /// <summary>
        /// 文本回答（主观题）
        /// </summary>
        [StringLength(5000)]
        public string TextAnswer { get; set; }

        /// <summary>
        /// 选择的选项IDs（JSON格式存储，用于单选、多选）
        /// </summary>
        [StringLength(1000)]
        public string SelectedOptionIds { get; set; }

        /// <summary>
        /// 排序后的选项IDs（JSON格式存储，用于排序题）
        /// </summary>
        [StringLength(1000)]
        public string SortedOptionIds { get; set; }

        /// <summary>
        /// 数值回答（用于数值输入题）
        /// </summary>
        public decimal? NumericAnswer { get; set; }

        /// <summary>
        /// 回答时间
        /// </summary>
        public DateTime ResponseTime { get; set; }

        /// <summary>
        /// 所属问卷回答
        /// </summary>
        public long QuestionnaireResponseId { get; set; }
        public virtual QuestionnaireResponse QuestionnaireResponse { get; set; }

        /// <summary>
        /// 所属题目
        /// </summary>
        public long QuestionId { get; set; }
        public virtual QuestionnaireQuestion Question { get; set; }
    }

    /// <summary>
    /// 题目类型
    /// </summary>
    public enum QuestionType
    {
        [Display(Name = "单选题")]
        SingleChoice,

        [Display(Name = "多选题")]
        MultipleChoice,

        [Display(Name = "排序题")]
        Ranking,

        [Display(Name = "多选排序题")]
        MultipleRanking,

        [Display(Name = "主观题")]
        Essay,

        [Display(Name = "数值输入")]
        Numeric,

        [Display(Name = "评分题")]
        Rating
    }

    /// <summary>
    /// 显示条件类型
    /// </summary>
    public enum ConditionType
    {
        [Display(Name = "选择了指定选项")]
        OptionSelected,

        [Display(Name = "没有选择指定选项")]
        OptionNotSelected,

        [Display(Name = "文本包含")]
        TextContains,

        [Display(Name = "文本不包含")]
        TextNotContains,

        [Display(Name = "数值等于")]
        NumericEquals,

        [Display(Name = "数值大于")]
        NumericGreaterThan,

        [Display(Name = "数值小于")]
        NumericLessThan,

        [Display(Name = "已回答")]
        Answered,

        [Display(Name = "未回答")]
        NotAnswered
    }

    /// <summary>
    /// 逻辑操作符
    /// </summary>
    public enum LogicalOperator
    {
        [Display(Name = "并且")]
        And,

        [Display(Name = "或者")]
        Or
    }
}
