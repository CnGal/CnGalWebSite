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

            // 检查用户是否已提交并加载用户答案
            if (!string.IsNullOrEmpty(userId))
            {
                viewModel.HasSubmitted = await HasUserSubmittedAsync(id, userId);
                if (viewModel.HasSubmitted)
                {
                    // 如果用户已提交过，无论是否允许修改都要加载之前的答案
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
                        Image = option.Image,
                        IsOtherOption = option.IsOtherOption
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

            // 如果用户已提交过，加载每个题目的用户答案
            if (viewModel.HasSubmitted && viewModel.UserResponse != null)
            {
                foreach (var question in viewModel.Questions)
                {
                    var userQuestionResponse = viewModel.UserResponse.QuestionResponses.FirstOrDefault(qr => qr.QuestionId == question.Id);
                    if (userQuestionResponse != null)
                    {
                        question.UserResponse = userQuestionResponse;
                    }
                }
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
                    EstimatedTimeMinutes = CalculateEstimatedTime(questionnaire.Questions.Count),
                    RequireLogin = questionnaire.RequireLogin
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

            // 获取问卷（包含显示条件）
            var questionnaire = await _questionnaireRepository.GetAll()
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.DisplayConditions)
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
            var existingResponse = await _responseRepository.GetAll()
                .Include(r => r.QuestionResponses)
                .FirstOrDefaultAsync(r => r.QuestionnaireId == model.QuestionnaireId &&
                                       r.ApplicationUserId == userId &&
                                       r.IsCompleted);

            QuestionnaireResponse questionnaireResponse;
            bool isNewSubmission = existingResponse == null;

            if (existingResponse != null)
            {
                if (!questionnaire.AllowMultipleSubmissions)
                {
                    return new Result { Successful = false, Error = "您已经提交过此问卷，不允许修改" };
                }
                else
                {
                    // 允许修改答案：累计用时而不是覆盖
                    var previousCompletionTime = existingResponse.CompletionTimeSeconds ?? 0;
                    var newCompletionTime = model.CompletionTimeSeconds ?? 0;

                    existingResponse.SubmitTime = DateTime.Now.ToCstTime();
                    existingResponse.IpAddress = ipAddress;
                    existingResponse.UserAgent = userAgent;
                    existingResponse.SessionId = model.SessionId;
                    existingResponse.CompletionTimeSeconds = previousCompletionTime + newCompletionTime; // 累计问卷总用时
                    questionnaireResponse = await _responseRepository.UpdateAsync(existingResponse);

                    // 保存原有题目回答的用时数据
                    var previousQuestionDurations = new Dictionary<long, int>();
                    var questionResponsesToDelete = await _questionResponseRepository.GetAll()
                        .Where(qr => qr.QuestionnaireResponseId == existingResponse.Id)
                        .ToListAsync();

                    foreach (var oldQuestionResponse in questionResponsesToDelete)
                    {
                        if (oldQuestionResponse.QuestionDurationSeconds.HasValue)
                        {
                            previousQuestionDurations[oldQuestionResponse.QuestionId] = oldQuestionResponse.QuestionDurationSeconds.Value;
                        }
                        await _questionResponseRepository.DeleteAsync(oldQuestionResponse);
                    }

                    // 将之前的用时数据传递给创建新回答的过程
                    foreach (var responseModel in model.Responses)
                    {
                        var previousDuration = previousQuestionDurations.GetValueOrDefault(responseModel.QuestionId, 0);
                        var newDuration = responseModel.QuestionDurationSeconds ?? 0;
                        responseModel.QuestionDurationSeconds = previousDuration + newDuration; // 累计题目用时
                    }
                }
            }
            else
            {
                // 验证必填题目（只检查应该显示的必填题目）
                var validationResult1 = ValidateRequiredVisibleQuestions(questionnaire, model.Responses);
                if (!validationResult1.Successful)
                {
                    return validationResult1;
                }

                // 创建新的问卷回答
                questionnaireResponse = new QuestionnaireResponse
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
            }

            // 验证必填题目（对于修改答案的情况也要验证）
            if (!isNewSubmission)
            {
                var validationResult2 = ValidateRequiredVisibleQuestions(questionnaire, model.Responses);
                if (!validationResult2.Successful)
                {
                    return validationResult2;
                }
            }

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
                    OtherOptionTexts = responseModel.OtherOptionTexts,
                    ResponseTime = DateTime.Now.ToCstTime(),
                    QuestionDurationSeconds = responseModel.QuestionDurationSeconds
                };

                await _questionResponseRepository.InsertAsync(questionResponse);
            }

            // 只有新提交时才增加回答数
            if (isNewSubmission)
            {
                questionnaire.ResponseCount++;
                await _questionnaireRepository.UpdateAsync(questionnaire);
            }

            // 提交成功后删除该用户的草稿（如果有的话）
            var draft = await _responseRepository.GetAll()
                .Include(r => r.QuestionResponses)
                .FirstOrDefaultAsync(r => r.QuestionnaireId == model.QuestionnaireId &&
                                       r.ApplicationUserId == userId &&
                                       !r.IsCompleted);

            if (draft != null)
            {
                // 删除草稿的题目回答
                var draftQuestionResponses = await _questionResponseRepository.GetAll()
                    .Where(qr => qr.QuestionnaireResponseId == draft.Id)
                    .ToListAsync();

                foreach (var draftResponse in draftQuestionResponses)
                {
                    await _questionResponseRepository.DeleteAsync(draftResponse);
                }

                // 删除草稿记录
                await _responseRepository.DeleteAsync(draft);
            }

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

            // 保存原有题目回答的用时数据，然后删除
            var previousQuestionDurations = new Dictionary<long, int>();
            var existingQuestionResponses = await _questionResponseRepository.GetAll()
                .Where(qr => qr.QuestionnaireResponseId == existingResponse.Id)
                .ToListAsync();

            foreach (var response in existingQuestionResponses)
            {
                if (response.QuestionDurationSeconds.HasValue)
                {
                    previousQuestionDurations[response.QuestionId] = response.QuestionDurationSeconds.Value;
                }
                await _questionResponseRepository.DeleteAsync(response);
            }

            // 将之前的用时数据累计到本次保存中
            foreach (var responseModel in model.Responses)
            {
                var previousDuration = previousQuestionDurations.GetValueOrDefault(responseModel.QuestionId, 0);
                var newDuration = responseModel.QuestionDurationSeconds ?? 0;
                responseModel.QuestionDurationSeconds = previousDuration + newDuration;
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
                    OtherOptionTexts = responseModel.OtherOptionTexts,
                    ResponseTime = DateTime.Now.ToCstTime(),
                    QuestionDurationSeconds = responseModel.QuestionDurationSeconds
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
            var completionTimes = completedResponses.Where(r => r.CompletionTimeSeconds.HasValue)
                .Select(r => r.CompletionTimeSeconds.Value).ToList();

            return new QuestionnaireStatisticsModel
            {
                QuestionnaireId = questionnaireId,
                TotalResponses = responses.Count,
                CompletedResponses = completedResponses.Count,
                DraftResponses = responses.Count - completedResponses.Count,
                CompletionRate = responses.Count > 0 ? (double)completedResponses.Count / responses.Count : 0,
                AverageCompletionTimeSeconds = completionTimes.Any() ? completionTimes.Average() : 0,
                MinCompletionTimeSeconds = completionTimes.Any() ? completionTimes.Min() : null,
                MaxCompletionTimeSeconds = completionTimes.Any() ? completionTimes.Max() : null,
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
            // 优先获取已完成的回答，如果没有则获取草稿
            var response = await _responseRepository.GetAll()
                .Include(r => r.QuestionResponses)
                .Where(r => r.QuestionnaireId == questionnaireId && r.ApplicationUserId == userId)
                .OrderByDescending(r => r.IsCompleted)  // 优先已完成的
                .ThenByDescending(r => r.SubmitTime)    // 然后按时间排序
                .FirstOrDefaultAsync();

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
                    OtherOptionTexts = questionResponse.OtherOptionTexts,
                    ResponseTime = questionResponse.ResponseTime,
                    QuestionId = questionResponse.QuestionId
                });
            }

            return viewModel;
        }

        #region 私有方法

        private Result ValidateRequiredVisibleQuestions(Questionnaire questionnaire, List<SubmitQuestionResponseModel> responses)
        {
            // 将提交的响应转换为条件验证所需的格式
            var questionResponses = ConvertToQuestionResponses(questionnaire, responses);

            var requiredQuestions = questionnaire.Questions.Where(q => q.IsRequired).ToList();

            foreach (var requiredQuestion in requiredQuestions)
            {
                // 检查题目是否应该显示
                var shouldDisplay = EvaluateDisplayConditions(requiredQuestion.DisplayConditions.ToList(), questionResponses);

                if (shouldDisplay)
                {
                    // 只有应该显示的必填题目才需要验证
                    var response = responses.FirstOrDefault(r => r.QuestionId == requiredQuestion.Id);
                    if (response == null || IsEmptyResponse(response, requiredQuestion.QuestionType))
                    {
                        return new Result { Successful = false, Error = $"题目{requiredQuestion.Title}为必填项" };
                    }
                }
            }

            return new Result { Successful = true };
        }

        private List<QuestionResponse> ConvertToQuestionResponses(Questionnaire questionnaire, List<SubmitQuestionResponseModel> responses)
        {
            var questionResponses = new List<QuestionResponse>();

            foreach (var response in responses)
            {
                var question = questionnaire.Questions.FirstOrDefault(q => q.Id == response.QuestionId);
                if (question == null) continue;

                var questionResponse = new QuestionResponse
                {
                    QuestionId = response.QuestionId,
                    TextAnswer = response.TextAnswer,
                    NumericAnswer = response.NumericAnswer,
                    SelectedOptionIds = response.SelectedOptionIds?.Count > 0 ? JsonSerializer.Serialize(response.SelectedOptionIds) : null,
                    SortedOptionIds = response.SortedOptionIds?.Count > 0 ? JsonSerializer.Serialize(response.SortedOptionIds) : null
                };

                questionResponses.Add(questionResponse);
            }

            return questionResponses;
        }

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

        public async Task<UserResponseListModel> GetUserResponseListAsync(long questionnaireId)
        {
            var questionnaire = await _questionnaireRepository.GetAll()
                .Where(q => q.Id == questionnaireId)
                .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                    .ThenInclude(qq => qq.DisplayConditions)
                .FirstOrDefaultAsync();

            if (questionnaire == null)
            {
                return null;
            }

            var responses = await _responseRepository.GetAll()
                .Where(r => r.QuestionnaireId == questionnaireId)
                .Include(r => r.QuestionResponses)
                .Include(r => r.ApplicationUser)
                .OrderByDescending(r => r.SubmitTime)
                .ToListAsync();

            var result = new UserResponseListModel
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireName = questionnaire.DisplayName
            };

            foreach (var response in responses)
            {
                var user = response.ApplicationUser;

                // 计算该用户的可见题目数量
                var visibleQuestionCount = CalculateVisibleQuestionCount(questionnaire.Questions.ToList(), response.QuestionResponses.ToList());

                result.Responses.Add(new UserResponseSummaryModel
                {
                    ResponseId = response.Id,
                    UserId = response.ApplicationUserId,
                    UserName = user?.UserName ?? "未知用户",
                    UserDisplayName = user?.UserName ?? "未知用户",
                    SubmitTime = response.SubmitTime,
                    IsCompleted = response.IsCompleted,
                    CompletionTimeSeconds = response.CompletionTimeSeconds,
                    IpAddress = response.IpAddress,
                    UserAgent = response.UserAgent,
                    AnsweredQuestionCount = response.QuestionResponses?.Count ?? 0,
                    TotalQuestionCount = questionnaire.Questions.Count,
                    VisibleQuestionCount = visibleQuestionCount
                });
            }

            return result;
        }

        private int CalculateVisibleQuestionCount(List<QuestionnaireQuestion> questions, List<QuestionResponse> userResponses)
        {
            int visibleCount = 0;

            foreach (var question in questions.OrderBy(q => q.SortOrder))
            {
                // 如果题目没有显示条件，则始终可见
                if (question.DisplayConditions == null || !question.DisplayConditions.Any())
                {
                    visibleCount++;
                    continue;
                }

                // 评估显示条件
                if (EvaluateDisplayConditions(question.DisplayConditions.ToList(), userResponses))
                {
                    visibleCount++;
                }
            }

            return visibleCount;
        }

        public async Task<UserResponseDetailModel> GetUserResponseDetailAsync(long responseId)
        {
            var response = await _responseRepository.GetAll()
                .Where(r => r.Id == responseId)
                .Include(r => r.QuestionResponses)
                .Include(r => r.ApplicationUser)
                .Include(r => r.Questionnaire)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(qq => qq.Options)
                .FirstOrDefaultAsync();

            if (response == null)
            {
                return null;
            }

            var user = response.ApplicationUser;
            var result = new UserResponseDetailModel
            {
                ResponseId = response.Id,
                UserId = response.ApplicationUserId,
                UserName = user?.UserName ?? "未知用户",
                UserDisplayName = user?.UserName ?? "未知用户",
                SubmitTime = response.SubmitTime,
                IsCompleted = response.IsCompleted,
                CompletionTimeSeconds = response.CompletionTimeSeconds,
                IpAddress = response.IpAddress,
                UserAgent = response.UserAgent
            };

            // 构建答案列表
            foreach (var question in response.Questionnaire.Questions.OrderBy(q => q.SortOrder))
            {
                var questionResponse = response.QuestionResponses.FirstOrDefault(qr => qr.QuestionId == question.Id);
                var answer = new UserQuestionAnswerModel
                {
                    QuestionId = question.Id,
                    QuestionTitle = question.Title,
                    QuestionType = question.QuestionType,
                    ResponseTime = questionResponse?.ResponseTime ?? response.SubmitTime,
                    QuestionDurationSeconds = questionResponse?.QuestionDurationSeconds
                                };

                if (questionResponse != null)
                {
                    answer.TextAnswer = questionResponse.TextAnswer;
                    answer.NumericAnswer = questionResponse.NumericAnswer;

                    // 处理选择题答案
                    if (!string.IsNullOrEmpty(questionResponse.SelectedOptionIds))
                    {
                        try
                        {
                            var selectedIds = JsonSerializer.Deserialize<List<long>>(questionResponse.SelectedOptionIds);
                            answer.SelectedOptions = question.Options
                                .Where(o => selectedIds.Contains(o.Id))
                                .OrderBy(o => o.SortOrder)
                                .Select(o => o.Text)
                                .ToList();
                        }
                        catch
                        {
                            // 如果反序列化失败，忽略
                        }
                    }

                    // 处理排序题答案
                    if (!string.IsNullOrEmpty(questionResponse.SortedOptionIds))
                    {
                        try
                        {
                            var sortedIds = JsonSerializer.Deserialize<List<long>>(questionResponse.SortedOptionIds);
                            answer.SortedOptions = sortedIds
                                .Select(id => question.Options.FirstOrDefault(o => o.Id == id)?.Text)
                                .Where(text => !string.IsNullOrEmpty(text))
                                .ToList();
                        }
                        catch
                        {
                            // 如果反序列化失败，忽略
                        }
                    }

                                                            // 处理"其他"选项的自定义文本
                    if (!string.IsNullOrEmpty(questionResponse.OtherOptionTexts))
                    {
                        try
                        {
                            var otherTexts = JsonSerializer.Deserialize<Dictionary<string, string>>(questionResponse.OtherOptionTexts);
                            if (otherTexts != null)
                            {
                                // 处理其他选项文本映射
                                foreach (var kvp in otherTexts)
                                {
                                    // 首先尝试根据选项Value查找（正确方式）
                                    var optionByValue = question.Options.FirstOrDefault(o => o.Value == kvp.Key);
                                    if (optionByValue != null)
                                    {
                                        answer.OtherOptionTexts[optionByValue.Text] = kvp.Value;
                                    }
                                    // 如果找不到，尝试根据选项ID查找（向后兼容）
                                    else if (long.TryParse(kvp.Key, out var optionId))
                                    {
                                        var optionById = question.Options.FirstOrDefault(o => o.Id == optionId);
                                        if (optionById != null)
                                        {
                                            answer.OtherOptionTexts[optionById.Text] = kvp.Value;
                                        }
                                        else
                                        {
                                            answer.OtherOptionTexts[$"选项ID:{optionId}"] = kvp.Value;
                                        }
                                    }
                                    else
                                    {
                                        // 如果键既不是有效的Value也不是数字ID，直接使用（向后兼容）
                                        answer.OtherOptionTexts[kvp.Key] = kvp.Value;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // 如果反序列化失败，忽略
                        }
                    }
                }

                result.Answers.Add(answer);
            }

            return result;
        }

        public async Task<QuestionAnalysisModel> GetQuestionAnalysisAsync(long questionnaireId)
        {
            var questionnaire = await _questionnaireRepository.GetAll()
                .Where(q => q.Id == questionnaireId)
                .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                    .ThenInclude(qq => qq.Options.OrderBy(o => o.SortOrder))
                .Include(q => q.Responses)
                    .ThenInclude(r => r.QuestionResponses)
                .FirstOrDefaultAsync();

            if (questionnaire == null)
            {
                return null;
            }

            var result = new QuestionAnalysisModel
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireName = questionnaire.DisplayName
            };

            var completedResponses = questionnaire.Responses.Where(r => r.IsCompleted).ToList();
            var totalResponseCount = completedResponses.Count;

            foreach (var question in questionnaire.Questions)
            {
                var questionStats = new QuestionStatisticsModel
                {
                    QuestionId = question.Id,
                    Title = question.Title,
                    Description = question.Description,
                    QuestionType = question.QuestionType,
                    IsRequired = question.IsRequired,
                    SortOrder = question.SortOrder,
                    TotalResponses = totalResponseCount
                };

                var questionResponses = completedResponses
                    .SelectMany(r => r.QuestionResponses)
                    .Where(qr => qr.QuestionId == question.Id)
                    .ToList();

                questionStats.ValidResponses = questionResponses.Count;

                switch (question.QuestionType)
                {
                    case QuestionType.SingleChoice:
                    case QuestionType.MultipleChoice:
                        AnalyzeChoiceQuestion(question, questionResponses, questionStats);
                        break;

                    case QuestionType.Essay:
                        AnalyzeEssayQuestion(questionResponses, questionStats);
                        break;

                    case QuestionType.Numeric:
                    case QuestionType.Rating:
                        AnalyzeNumericQuestion(questionResponses, questionStats);
                        break;

                    case QuestionType.Ranking:
                    case QuestionType.MultipleRanking:
                        AnalyzeRankingQuestion(question, questionResponses, questionStats);
                        break;
                }

                // 分析用时统计
                AnalyzeQuestionDuration(questionResponses, questionStats);

                result.Questions.Add(questionStats);
            }

            return result;
        }

        private void AnalyzeChoiceQuestion(QuestionnaireQuestion question, List<QuestionResponse> responses, QuestionStatisticsModel stats)
        {
            var optionCounts = new Dictionary<long, int>();
            var otherTexts = new Dictionary<long, List<string>>();
            var totalSelections = 0;

            foreach (var response in responses)
            {
                if (!string.IsNullOrEmpty(response.SelectedOptionIds))
                {
                    try
                    {
                        var selectedIds = JsonSerializer.Deserialize<List<long>>(response.SelectedOptionIds);
                        foreach (var optionId in selectedIds)
                        {
                            optionCounts[optionId] = optionCounts.GetValueOrDefault(optionId, 0) + 1;
                            totalSelections++;
                        }
                    }
                    catch
                    {
                        // 忽略解析错误
                    }
                }

                // 收集"其他"选项的自定义文本
                if (!string.IsNullOrEmpty(response.OtherOptionTexts))
                {
                    try
                    {
                        var responseOtherTexts = JsonSerializer.Deserialize<Dictionary<string, string>>(response.OtherOptionTexts);
                        if (responseOtherTexts != null)
                        {
                            foreach (var kvp in responseOtherTexts)
                            {
                                if (long.TryParse(kvp.Key, out var optionId) && !string.IsNullOrWhiteSpace(kvp.Value))
                                {
                                    if (!otherTexts.ContainsKey(optionId))
                                    {
                                        otherTexts[optionId] = new List<string>();
                                    }
                                    otherTexts[optionId].Add(kvp.Value);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 忽略解析错误
                    }
                }
            }

            foreach (var option in question.Options)
            {
                var count = optionCounts.GetValueOrDefault(option.Id, 0);
                var percentage = question.QuestionType == QuestionType.SingleChoice && stats.ValidResponses > 0
                    ? (double)count / stats.ValidResponses * 100
                    : totalSelections > 0 ? (double)count / totalSelections * 100 : 0;

                var optionStat = new OptionStatisticsModel
                {
                    OptionId = option.Id,
                    OptionText = option.Text,
                    OptionValue = option.Value,
                    SelectionCount = count,
                    SelectionPercentage = percentage,
                    IsOtherOption = option.IsOtherOption,
                    OtherTexts = otherTexts.GetValueOrDefault(option.Id, new List<string>())
                };

                stats.OptionStatistics.Add(optionStat);
            }
        }

        private void AnalyzeEssayQuestion(List<QuestionResponse> responses, QuestionStatisticsModel stats)
        {
            stats.TextAnswers = responses
                .Where(r => !string.IsNullOrWhiteSpace(r.TextAnswer))
                .Select(r => r.TextAnswer)
                .ToList();
        }

        private void AnalyzeNumericQuestion(List<QuestionResponse> responses, QuestionStatisticsModel stats)
        {
            var numericValues = responses
                .Where(r => r.NumericAnswer.HasValue)
                .Select(r => r.NumericAnswer.Value)
                .ToList();

            if (numericValues.Any())
            {
                stats.NumericValues = numericValues;
                stats.AverageNumericValue = numericValues.Average();
                stats.MinNumericValue = numericValues.Min();
                stats.MaxNumericValue = numericValues.Max();
            }
        }

        private void AnalyzeRankingQuestion(QuestionnaireQuestion question, List<QuestionResponse> responses, QuestionStatisticsModel stats)
        {
            var rankingData = new Dictionary<long, List<int>>();
            var otherTexts = new Dictionary<long, List<string>>();

            foreach (var response in responses)
            {
                if (!string.IsNullOrEmpty(response.SortedOptionIds))
                {
                    try
                    {
                        var sortedIds = JsonSerializer.Deserialize<List<long>>(response.SortedOptionIds);
                        for (int i = 0; i < sortedIds.Count; i++)
                        {
                            var optionId = sortedIds[i];
                            if (!rankingData.ContainsKey(optionId))
                            {
                                rankingData[optionId] = new List<int>();
                            }
                            rankingData[optionId].Add(i + 1); // 排名从1开始
                        }
                    }
                    catch
                    {
                        // 忽略解析错误
                    }
                }

                // 收集排序题中"其他"选项的自定义文本
                if (!string.IsNullOrEmpty(response.OtherOptionTexts))
                {
                    try
                    {
                        var responseOtherTexts = JsonSerializer.Deserialize<Dictionary<string, string>>(response.OtherOptionTexts);
                        if (responseOtherTexts != null)
                        {
                            foreach (var kvp in responseOtherTexts)
                            {
                                if (long.TryParse(kvp.Key, out var optionId) && !string.IsNullOrWhiteSpace(kvp.Value))
                                {
                                    if (!otherTexts.ContainsKey(optionId))
                                    {
                                        otherTexts[optionId] = new List<string>();
                                    }
                                    otherTexts[optionId].Add(kvp.Value);
                                }
                                else
                                {
                                    // 处理选项Value作为键的情况
                                    var option = question.Options.FirstOrDefault(o => o.Value == kvp.Key);
                                    if (option != null && !string.IsNullOrWhiteSpace(kvp.Value))
                                    {
                                        if (!otherTexts.ContainsKey(option.Id))
                                        {
                                            otherTexts[option.Id] = new List<string>();
                                        }
                                        otherTexts[option.Id].Add(kvp.Value);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // 忽略解析错误
                    }
                }
            }

            foreach (var option in question.Options)
            {
                var ranks = rankingData.GetValueOrDefault(option.Id, new List<int>());
                var rankCounts = new Dictionary<int, int>();

                foreach (var rank in ranks)
                {
                    rankCounts[rank] = rankCounts.GetValueOrDefault(rank, 0) + 1;
                }

                stats.RankingStatistics.Add(new RankingStatisticsModel
                {
                    OptionId = option.Id,
                    OptionText = option.Text,
                    AverageRank = ranks.Any() ? ranks.Average() : 0,
                    RankPositions = ranks,
                    RankCounts = rankCounts,
                    IsOtherOption = option.IsOtherOption,
                    OtherTexts = otherTexts.GetValueOrDefault(option.Id, new List<string>())
                });
            }
        }

        private void AnalyzeQuestionDuration(List<QuestionResponse> responses, QuestionStatisticsModel stats)
        {
            var durations = responses
                .Where(r => r.QuestionDurationSeconds.HasValue && r.QuestionDurationSeconds.Value > 0)
                .Select(r => r.QuestionDurationSeconds.Value)
                .ToList();

            if (durations.Any())
            {
                stats.QuestionDurations = durations;
                stats.AverageQuestionDurationSeconds = durations.Average();
                stats.MinQuestionDurationSeconds = durations.Min();
                stats.MaxQuestionDurationSeconds = durations.Max();
            }
        }

        #endregion
    }
}
