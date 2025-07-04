using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Questionnaires
{
    /// <summary>
    /// 提交问卷的模型
    /// </summary>
    public class SubmitQuestionnaireModel
    {
        [Required]
        public long QuestionnaireId { get; set; }

        public string SessionId { get; set; }

        public int? CompletionTimeSeconds { get; set; }

        [Required]
        public List<SubmitQuestionResponseModel> Responses { get; set; } = new List<SubmitQuestionResponseModel>();

        public Result Validate()
        {
            if (QuestionnaireId <= 0)
            {
                return new Result { Successful = false, Error = "问卷ID无效" };
            }

            if (Responses == null || Responses.Count == 0)
            {
                return new Result { Successful = false, Error = "至少需要回答一个题目" };
            }

            foreach (var response in Responses)
            {
                var result = response.Validate();
                if (!result.Successful)
                {
                    return result;
                }
            }

            return new Result { Successful = true };
        }
    }

    /// <summary>
    /// 提交题目回答的模型
    /// </summary>
    public class SubmitQuestionResponseModel
    {
        [Required]
        public long QuestionId { get; set; }

        public string TextAnswer { get; set; }

        public List<long> SelectedOptionIds { get; set; } = new List<long>();

        public List<long> SortedOptionIds { get; set; } = new List<long>();

        public decimal? NumericAnswer { get; set; }

        /// <summary>
        /// "其他"选项的自定义文本（JSON格式存储）
        /// </summary>
        public string OtherOptionTexts { get; set; }

        /// <summary>
        /// 题目用时（秒）
        /// </summary>
        public int? QuestionDurationSeconds { get; set; }

        public Result Validate()
        {
            if (QuestionId <= 0)
            {
                return new Result { Successful = false, Error = "题目ID无效" };
            }

            return new Result { Successful = true };
        }
    }

    /// <summary>
    /// 保存草稿的模型
    /// </summary>
    public class SaveDraftModel
    {
        [Required]
        public long QuestionnaireId { get; set; }

        public List<SubmitQuestionResponseModel> Responses { get; set; } = new List<SubmitQuestionResponseModel>();
    }
}
