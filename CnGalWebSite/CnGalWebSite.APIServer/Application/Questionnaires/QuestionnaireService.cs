using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Questionnaires;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Questionnaires
{
    public class QuestionnaireService : IQuestionnaireService
    {
        private readonly IRepository<Questionnaire, long> _questionnaireRepository;
        private readonly IRepository<QuestionnaireQuestion, long> _questionRepository;
        private readonly IRepository<QuestionOption, long> _optionRepository;
        private readonly IRepository<QuestionDisplayCondition, long> _conditionRepository;
        private readonly IRepository<QuestionnaireResponse, long> _responseRepository;
        private readonly IRepository<QuestionResponse, long> _questionResponseRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IAppHelper _appHelper;

        public QuestionnaireService(
            IRepository<Questionnaire, long> questionnaireRepository,
            IRepository<QuestionnaireQuestion, long> questionRepository,
            IRepository<QuestionOption, long> optionRepository,
            IRepository<QuestionDisplayCondition, long> conditionRepository,
            IRepository<QuestionnaireResponse, long> responseRepository,
            IRepository<QuestionResponse, long> questionResponseRepository,
            IRepository<ApplicationUser, string> userRepository,
            IAppHelper appHelper)
        {
            _questionnaireRepository = questionnaireRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _conditionRepository = conditionRepository;
            _responseRepository = responseRepository;
            _questionResponseRepository = questionResponseRepository;
            _userRepository = userRepository;
            _appHelper = appHelper;
        }

        public async Task<QuestionnaireViewModel> GetQuestionnaireDetailAsync(long id, string userId = null)
        {
            var questionnaire = await _questionnaireRepository.GetAll()
                .Where(q => q.Id == id && !q.IsHidden)
                .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                    .ThenInclude(qq => qq.Options.OrderBy(o => o.SortOrder))
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.DisplayConditions)
                .FirstOrDefaultAsync();

            if (questionnaire == null)
            {
                return null;
            }

            var viewModel = new QuestionnaireViewModel
            {
                Id = questionnaire.Id,
                Name = questionnaire.Name,
                DisplayName = questionnaire.DisplayName,
                Description = questionnaire.Description,
                MainPicture = questionnaire.MainPicture,
                Thumbnail = questionnaire.Thumbnail,
                CreateTime = questionnaire.CreateTime,
                LastEditTime = questionnaire.LastEditTime,
                BeginTime = questionnaire.BeginTime,
                EndTime = questionnaire.EndTime,
                IsActive = questionnaire.IsActive,
                AllowMultipleSubmissions = questionnaire.AllowMultipleSubmissions,
                RequireLogin = questionnaire.RequireLogin,
                ThankYouMessage = questionnaire.ThankYouMessage,
                ResponseCount = questionnaire.ResponseCount
            };

            // 检查用户是否已提交
            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.HasSubmitted = await HasUserSubmittedAsync(id, userId);
                if (viewModel.HasSubmitted && !questionnaire.AllowMultipleSubmissions)
                {
                    viewModel.UserResponse = await GetUserResponseAsync(id, userId);
                }
            }

            // 映射题目
            foreach (var question in questionnaire.Questions)
            {
                var questionViewModel = new QuestionnaireQuestionViewModel
                {
                    Id = question.Id,
                    Title = question.Title,
                    Description = question.Description,
                    QuestionType = question.QuestionType,
                    IsRequired = question.IsRequired,
                    SortOrder = question.SortOrder,
                    MinSelectionCount = question.MinSelectionCount,
                    MaxSelectionCount = question.MaxSelectionCount,
                    MaxTextLength = question.MaxTextLength,
                    TextPlaceholder = question.TextPlaceholder
                };

                // 映射选项
                foreach (var option in question.Options)
                {
                    questionViewModel.Options.Add(new QuestionOptionViewModel
                    {
                        Id = option.Id,
                        Text = option.Text,
                        Value = option.Value,
                        SortOrder = option.SortOrder,
                        IsEnabled = option.IsEnabled,
                        Image = option.Image
                    });
                }

                // 映射显示条件
                foreach (var condition in question.DisplayConditions)
                {
                    questionViewModel.DisplayConditions.Add(new QuestionDisplayConditionViewModel
                    {
                        Id = condition.Id,
                        ConditionType = condition.ConditionType,
                        LogicalOperator = condition.LogicalOperator,
                        TriggerQuestionId = condition.TriggerQuestionId,
                        TriggerOptionId = condition.TriggerOptionId,
                        ExpectedValue = condition.ExpectedValue,
                        ConditionGroup = condition.ConditionGroup
                    });
                }

                viewModel.Questions.Add(questionViewModel);
            }

            return viewModel;
        }

        public async Task<List<QuestionnaireCardViewModel>> GetQuestionnaireCardsAsync(string userId = null)
        {
            var questionnaires = await _questionnaireRepository.GetAll()
                .Where(q => !q.IsHidden)
                .Include(q => q.Questions)
                .OrderByDescending(q => q.Priority)
                .ThenByDescending(q => q.CreateTime)
                .ToListAsync();

            var cards = new List<QuestionnaireCardViewModel>();

            foreach (var questionnaire in questionnaires)
            {
                var status = GetQuestionnaireStatus(questionnaire);
                var hasParticipated = false;

                if (!string.IsNullOrEmpty(userId))
                {
                    hasParticipated = await HasUserSubmittedAsync(questionnaire.Id, userId);
                }

                cards.Add(new QuestionnaireCardViewModel
                {
                    Id = questionnaire.Id,
                    Name = questionnaire.Name,
                    DisplayName = questionnaire.DisplayName,
                    Description = questionnaire.Description,
                    Thumbnail = questionnaire.Thumbnail,
                    CreateTime = questionnaire.CreateTime,
                    BeginTime = questionnaire.BeginTime,
                    EndTime = questionnaire.EndTime,
                    IsActive = questionnaire.IsActive,
                    ResponseCount = questionnaire.ResponseCount,
                    Priority = questionnaire.Priority,
                    Status = status,
                    HasParticipated = hasParticipated,
                    AllowMultipleSubmissions = questionnaire.AllowMultipleSubmissions,
                    QuestionCount = questionnaire.Questions.Count,
                    EstimatedTimeMinutes = CalculateEstimatedTime(questionnaire.Questions.Count)
                });
            }

            return cards;
        }

        public async Task<Result> SubmitQuestionnaireAsync(SubmitQuestionnaireModel model, string userId, string ipAddress, string userAgent)
        {
            // 验证模型
            var validationResult = model.Validate();
            if (!validationResult.Successful)
            {
                return validationResult;
            }

            // 获取问卷
            var questionnaire = await _questionnaireRepository.GetAll()
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .FirstOrDefaultAsync(q => q.Id == model.QuestionnaireId);

            if (questionnaire == null)
            {
                return new Result { Successful = false, Error = "问卷不存在" };
            }

            if (!questionnaire.IsActive)
            {
                return new Result { Successful = false, Error = "问卷未激活" };
            }

            if (questionnaire.EndTime.HasValue && DateTime.Now.ToCstTime() > questionnaire.EndTime)
            {
                return new Result { Successful = false, Error = "问卷已结束" };
            }

            if (questionnaire.BeginTime.HasValue && DateTime.Now.ToCstTime() < questionnaire.BeginTime)
            {
                return new Result { Successful = false, Error = "问卷尚未开始" };
            }

            // 检查用户是否已提交
            if (!questionnaire.AllowMultipleSubmissions && !string.IsNullOrEmpty(userId))
            {
                var hasSubmitted = await HasUserSubmittedAsync(model.QuestionnaireId, userId);
                if (hasSubmitted)
                {
                    return new Result { Successful = false, Error = "您已经提交过此问卷" };
                }
            }

            // 验证必填题目
            var requiredQuestions = questionnaire.Questions.Where(q => q.IsRequired).ToList();
            foreach (var requiredQuestion in requiredQuestions)
            {
                var response = model.Responses.FirstOrDefault(r => r.QuestionId == requiredQuestion.Id);
                if (response == null || IsEmptyResponse(response, requiredQuestion.QuestionType))
                {
                    return new Result { Successful = false, Error = $"题目{requiredQuestion.Title}为必填项" };
                }
            }

            // 创建问卷回答
            var questionnaireResponse = new QuestionnaireResponse
            {
                QuestionnaireId = model.QuestionnaireId,
                ApplicationUserId = userId,
                SubmitTime = DateTime.Now.ToCstTime(),
                IsCompleted = true,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                SessionId = model.SessionId,
                CompletionTimeSeconds = model.CompletionTimeSeconds
            };

            questionnaireResponse = await _responseRepository.InsertAsync(questionnaireResponse);

            // 创建题目回答
            foreach (var responseModel in model.Responses)
            {
                var questionResponse = new QuestionResponse
                {
                    QuestionnaireResponseId = questionnaireResponse.Id,
                    QuestionId = responseModel.QuestionId,
                    TextAnswer = responseModel.TextAnswer,
                    SelectedOptionIds = responseModel.SelectedOptionIds.Count > 0 ? JsonSerializer.Serialize(responseModel.SelectedOptionIds) : null,
                    SortedOptionIds = responseModel.SortedOptionIds.Count > 0 ? JsonSerializer.Serialize(responseModel.SortedOptionIds) : null,
                    NumericAnswer = responseModel.NumericAnswer,
                    ResponseTime = DateTime.Now.ToCstTime()
                };

                await _questionResponseRepository.InsertAsync(questionResponse);
            }

            // 更新问卷回答数
            questionnaire.ResponseCount++;
            await _questionnaireRepository.UpdateAsync(questionnaire);

            return new Result { Successful = true };
        }

        public async Task<Result> SaveDraftAsync(SaveDraftModel model, string userId)
        {
            // 查找或创建草稿
            var existingResponse = await _responseRepository.GetAll()
                .FirstOrDefaultAsync(r => r.QuestionnaireId == model.QuestionnaireId &&
                                        r.ApplicationUserId == userId &&
                                        !r.IsCompleted);

            if (existingResponse == null)
            {
                existingResponse = new QuestionnaireResponse
                {
                    QuestionnaireId = model.QuestionnaireId,
                    ApplicationUserId = userId,
                    SubmitTime = DateTime.Now.ToCstTime(),
                    IsCompleted = false
                };
                existingResponse = await _responseRepository.InsertAsync(existingResponse);
            }

            // 删除现有的题目回答
            var existingQuestionResponses = await _questionResponseRepository.GetAll()
                .Where(qr => qr.QuestionnaireResponseId == existingResponse.Id)
                .ToListAsync();

            foreach (var response in existingQuestionResponses)
            {
                await _questionResponseRepository.DeleteAsync(response);
            }

            // 创建新的题目回答
            foreach (var responseModel in model.Responses)
            {
                var questionResponse = new QuestionResponse
                {
                    QuestionnaireResponseId = existingResponse.Id,
                    QuestionId = responseModel.QuestionId,
                    TextAnswer = responseModel.TextAnswer,
                    SelectedOptionIds = responseModel.SelectedOptionIds.Count > 0 ? JsonSerializer.Serialize(responseModel.SelectedOptionIds) : null,
                    SortedOptionIds = responseModel.SortedOptionIds.Count > 0 ? JsonSerializer.Serialize(responseModel.SortedOptionIds) : null,
                    NumericAnswer = responseModel.NumericAnswer,
                    ResponseTime = DateTime.Now.ToCstTime()
                };

                await _questionResponseRepository.InsertAsync(questionResponse);
            }

            return new Result { Successful = true };
        }

        public bool EvaluateDisplayConditions(List<QuestionDisplayCondition> conditions, List<QuestionResponse> responses)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return true; // 没有条件则显示
            }

            // 按条件组分组
            var conditionGroups = conditions.GroupBy(c => c.ConditionGroup);

            foreach (var group in conditionGroups)
            {
                var groupResult = true;
                var groupConditions = group.OrderBy(c => c.Id).ToList();

                for (int i = 0; i < groupConditions.Count; i++)
                {
                    var condition = groupConditions[i];
                    var conditionResult = EvaluateSingleCondition(condition, responses);

                    if (i == 0)
                    {
                        groupResult = conditionResult;
                    }
                    else
                    {
                        if (condition.LogicalOperator == LogicalOperator.And)
                        {
                            groupResult = groupResult && conditionResult;
                        }
                        else // Or
                        {
                            groupResult = groupResult || conditionResult;
                        }
                    }
                }

                if (groupResult)
                {
                    return true; // 任何一个组满足条件就显示
                }
            }

            return false;
        }

        public async Task<QuestionnaireStatisticsModel> GetQuestionnaireStatisticsAsync(long questionnaireId)
        {
            var responses = await _responseRepository.GetAll()
                .Where(r => r.QuestionnaireId == questionnaireId)
                .ToListAsync();

            var completedResponses = responses.Where(r => r.IsCompleted).ToList();

            return new QuestionnaireStatisticsModel
            {
                QuestionnaireId = questionnaireId,
                TotalResponses = responses.Count,
                CompletedResponses = completedResponses.Count,
                DraftResponses = responses.Count - completedResponses.Count,
                CompletionRate = responses.Count > 0 ? (double)completedResponses.Count / responses.Count : 0,
                AverageCompletionTimeSeconds = completedResponses.Where(r => r.CompletionTimeSeconds.HasValue)
                    .Select(r => r.CompletionTimeSeconds.Value).DefaultIfEmpty(0).Average(),
                FirstResponseTime = responses.Count > 0 ? responses.Min(r => r.SubmitTime) : null,
                LastResponseTime = responses.Count > 0 ? responses.Max(r => r.SubmitTime) : null
            };
        }

        public async Task<bool> HasUserSubmittedAsync(long questionnaireId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            return await _responseRepository.GetAll()
                .AnyAsync(r => r.QuestionnaireId == questionnaireId &&
                            r.ApplicationUserId == userId &&
                            r.IsCompleted);
        }

        public async Task<QuestionnaireResponseViewModel> GetUserResponseAsync(long questionnaireId, string userId)
        {
            var response = await _responseRepository.GetAll()
                .Include(r => r.QuestionResponses)
                .FirstOrDefaultAsync(r => r.QuestionnaireId == questionnaireId &&
                                       r.ApplicationUserId == userId);

            if (response == null)
            {
                return null;
            }

            var viewModel = new QuestionnaireResponseViewModel
            {
                Id = response.Id,
                SubmitTime = response.SubmitTime,
                IsCompleted = response.IsCompleted,
                CompletionTimeSeconds = response.CompletionTimeSeconds,
                ApplicationUserId = response.ApplicationUserId
            };

            foreach (var questionResponse in response.QuestionResponses)
            {
                var selectedOptionIds = new List<long>();
                var sortedOptionIds = new List<long>();

                if (!string.IsNullOrEmpty(questionResponse.SelectedOptionIds))
                {
                    selectedOptionIds = JsonSerializer.Deserialize<List<long>>(questionResponse.SelectedOptionIds);
                }

                if (!string.IsNullOrEmpty(questionResponse.SortedOptionIds))
                {
                    sortedOptionIds = JsonSerializer.Deserialize<List<long>>(questionResponse.SortedOptionIds);
                }

                viewModel.QuestionResponses.Add(new QuestionResponseViewModel
                {
                    Id = questionResponse.Id,
                    TextAnswer = questionResponse.TextAnswer,
                    SelectedOptionIds = selectedOptionIds,
                    SortedOptionIds = sortedOptionIds,
                    NumericAnswer = questionResponse.NumericAnswer,
                    ResponseTime = questionResponse.ResponseTime,
                    QuestionId = questionResponse.QuestionId
                });
            }

            return viewModel;
        }

        #region 私有方法

        private static QuestionnaireStatus GetQuestionnaireStatus(Questionnaire questionnaire)
        {
            if (!questionnaire.IsActive)
            {
                return QuestionnaireStatus.Inactive;
            }

            var now = DateTime.Now.ToCstTime();

            if (questionnaire.BeginTime.HasValue && now < questionnaire.BeginTime)
            {
                return QuestionnaireStatus.NotStarted;
            }

            if (questionnaire.EndTime.HasValue && now > questionnaire.EndTime)
            {
                return QuestionnaireStatus.Ended;
            }

            return QuestionnaireStatus.Active;
        }

        private int CalculateEstimatedTime(int questionCount)
        {
            // 简单估算：每题1分钟
            return Math.Max(1, questionCount);
        }

        private bool IsEmptyResponse(SubmitQuestionResponseModel response, QuestionType questionType)
        {
            switch (questionType)
            {
                case QuestionType.Essay:
                    return string.IsNullOrWhiteSpace(response.TextAnswer);
                case QuestionType.Numeric:
                case QuestionType.Rating:
                    return !response.NumericAnswer.HasValue;
                case QuestionType.SingleChoice:
                case QuestionType.MultipleChoice:
                    return response.SelectedOptionIds == null || response.SelectedOptionIds.Count == 0;
                case QuestionType.Ranking:
                case QuestionType.MultipleRanking:
                    return response.SortedOptionIds == null || response.SortedOptionIds.Count == 0;
                default:
                    return true;
            }
        }

        private bool EvaluateSingleCondition(QuestionDisplayCondition condition, List<QuestionResponse> responses)
        {
            var response = responses.FirstOrDefault(r => r.QuestionId == condition.TriggerQuestionId);
            if (response == null)
            {
                return condition.ConditionType == ConditionType.NotAnswered;
            }

            switch (condition.ConditionType)
            {
                case ConditionType.OptionSelected:
                    if (condition.TriggerOptionId.HasValue)
                    {
                        var selectedOptionIds = JsonSerializer.Deserialize<List<long>>(response.SelectedOptionIds ?? "[]");
                        return selectedOptionIds.Contains(condition.TriggerOptionId.Value);
                    }
                    return false;

                case ConditionType.OptionNotSelected:
                    if (condition.TriggerOptionId.HasValue)
                    {
                        var selectedOptionIds = JsonSerializer.Deserialize<List<long>>(response.SelectedOptionIds ?? "[]");
                        return !selectedOptionIds.Contains(condition.TriggerOptionId.Value);
                    }
                    return true;

                case ConditionType.TextContains:
                    return !string.IsNullOrEmpty(response.TextAnswer) &&
                           response.TextAnswer.Contains(condition.ExpectedValue ?? "");

                case ConditionType.TextNotContains:
                    return string.IsNullOrEmpty(response.TextAnswer) ||
                           !response.TextAnswer.Contains(condition.ExpectedValue ?? "");

                case ConditionType.NumericEquals:
                    if (decimal.TryParse(condition.ExpectedValue, out var expectedValue))
                    {
                        return response.NumericAnswer.HasValue && response.NumericAnswer.Value == expectedValue;
                    }
                    return false;

                case ConditionType.NumericGreaterThan:
                    if (decimal.TryParse(condition.ExpectedValue, out var greaterValue))
                    {
                        return response.NumericAnswer.HasValue && response.NumericAnswer.Value > greaterValue;
                    }
                    return false;

                case ConditionType.NumericLessThan:
                    if (decimal.TryParse(condition.ExpectedValue, out var lessValue))
                    {
                        return response.NumericAnswer.HasValue && response.NumericAnswer.Value < lessValue;
                    }
                    return false;

                case ConditionType.Answered:
                    return true; // 已经有回答记录

                case ConditionType.NotAnswered:
                    return false; // 已经有回答记录

                default:
                    return false;
            }
        }

        #endregion
    }
}
