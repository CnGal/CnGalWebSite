using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Questionnaires;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Questionnaires;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/questionnaires/[action]")]
    public class QuestionnaireAPIController : ControllerBase
    {
        private readonly IRepository<Questionnaire, long> _questionnaireRepository;
        private readonly IRepository<QuestionnaireQuestion, long> _questionRepository;
        private readonly IRepository<QuestionOption, long> _optionRepository;
        private readonly IRepository<QuestionDisplayCondition, long> _conditionRepository;
        private readonly IRepository<QuestionnaireResponse, long> _responseRepository;
        private readonly IRepository<QuestionResponse, long> _questionResponseRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IQuestionnaireService _questionnaireService;
        private readonly IAppHelper _appHelper;
        private readonly ILogger<QuestionnaireAPIController> _logger;
        private readonly IOperationRecordService _operationRecordService;
        private readonly IQueryService _queryService;

        public QuestionnaireAPIController(
            IRepository<Questionnaire, long> questionnaireRepository,
            IRepository<QuestionnaireQuestion, long> questionRepository,
            IRepository<QuestionOption, long> optionRepository,
            IRepository<QuestionDisplayCondition, long> conditionRepository,
            IRepository<QuestionnaireResponse, long> responseRepository,
            IRepository<QuestionResponse, long> questionResponseRepository,
            IRepository<ApplicationUser, string> userRepository,
            IQuestionnaireService questionnaireService,
            IAppHelper appHelper,
            ILogger<QuestionnaireAPIController> logger,
            IOperationRecordService operationRecordService,
            IQueryService queryService)
        {
            _questionnaireRepository = questionnaireRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _conditionRepository = conditionRepository;
            _responseRepository = responseRepository;
            _questionResponseRepository = questionResponseRepository;
            _userRepository = userRepository;
            _questionnaireService = questionnaireService;
            _appHelper = appHelper;
            _logger = logger;
            _operationRecordService = operationRecordService;
            _queryService = queryService;
        }

        #region 公开接口

        /// <summary>
        /// 获取问卷详情
        /// </summary>
        /// <param name="id">问卷ID</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionnaireViewModel>> GetQuestionnaireAsync(long id)
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                var questionnaire = await _questionnaireService.GetQuestionnaireDetailAsync(id, user?.Id);

                if (questionnaire == null)
                {
                    return NotFound("未找到指定的问卷");
                }

                // 检查问卷是否可访问
                if (!questionnaire.IsActive)
                {
                    return BadRequest("问卷未激活");
                }

                var now = DateTime.Now.ToCstTime();
                if (questionnaire.BeginTime.HasValue && now < questionnaire.BeginTime)
                {
                    return BadRequest("问卷尚未开始");
                }

                if (questionnaire.EndTime.HasValue && now > questionnaire.EndTime)
                {
                    return BadRequest("问卷已结束");
                }

                return Ok(questionnaire);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取问卷详情时发生错误，问卷ID: {QuestionnaireId}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取问卷列表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<QuestionnaireCardViewModel>>> GetQuestionnaireCardsAsync()
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                var cards = await _questionnaireService.GetQuestionnaireCardsAsync(user?.Id);
                return Ok(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取问卷列表时发生错误");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 提交问卷
        /// </summary>
        /// <param name="model">提交模型</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> SubmitQuestionnaireAsync(SubmitQuestionnaireModel model)
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    return BadRequest("用户未登录");
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var result = await _questionnaireService.SubmitQuestionnaireAsync(model, user.Id, ipAddress, userAgent);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交问卷时发生错误，问卷ID: {QuestionnaireId}", model?.QuestionnaireId);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 保存草稿
        /// </summary>
        /// <param name="model">草稿模型</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> SaveDraftAsync(SaveDraftModel model)
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    return BadRequest("用户未登录");
                }

                var result = await _questionnaireService.SaveDraftAsync(model, user.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存草稿时发生错误，问卷ID: {QuestionnaireId}", model?.QuestionnaireId);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取用户的问卷回答
        /// </summary>
        /// <param name="questionnaireId">问卷ID</param>
        /// <returns></returns>
        [HttpGet("{questionnaireId}")]
        public async Task<ActionResult<QuestionnaireResponseViewModel>> GetUserResponseAsync(long questionnaireId)
        {
            try
            {
                var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    return BadRequest("用户未登录");
                }

                var response = await _questionnaireService.GetUserResponseAsync(questionnaireId, user.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户回答时发生错误，问卷ID: {QuestionnaireId}", questionnaireId);
                return StatusCode(500, "服务器内部错误");
            }
        }

        #endregion

        #region 管理员接口

        /// <summary>
        /// 创建问卷
        /// </summary>
        /// <param name="model">创建模型</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> CreateQuestionnaireAsync(EditQuestionnaireModel model)
        {
            try
            {
                var validationResult = model.Validate();
                if (!validationResult.Successful)
                {
                    return BadRequest(validationResult);
                }

                // 检查名称唯一性
                var existingQuestionnaire = await _questionnaireRepository.GetAll()
                    .FirstOrDefaultAsync(q => q.Name == model.Name);

                if (existingQuestionnaire != null)
                {
                    return BadRequest(new Result { Successful = false, Error = "问卷名称已存在" });
                }

                // 创建问卷
                var questionnaire = new Questionnaire
                {
                    Name = model.Name,
                    DisplayName = model.DisplayName,
                    Description = model.Description,
                    MainPicture = model.MainPicture,
                    Thumbnail = model.Thumbnail,
                    BeginTime = model.BeginTime,
                    EndTime = model.EndTime,
                    IsActive = model.IsActive,
                    AllowMultipleSubmissions = model.AllowMultipleSubmissions,
                    RequireLogin = model.RequireLogin,
                    Priority = model.Priority,
                    ThankYouMessage = model.ThankYouMessage,
                    CreateTime = DateTime.Now.ToCstTime(),
                    LastEditTime = DateTime.Now.ToCstTime()
                };

                questionnaire = await _questionnaireRepository.InsertAsync(questionnaire);

                // 创建题目
                await CreateOrUpdateQuestions(questionnaire.Id, model.Questions);

                return Ok(new Result { Successful = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建问卷时发生错误");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取问卷编辑信息
        /// </summary>
        /// <param name="id">问卷ID</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<EditQuestionnaireModel>> EditQuestionnaireAsync(long id)
        {
            try
            {
                var questionnaire = await _questionnaireRepository.GetAll()
                    .Include(q => q.Questions.OrderBy(qq => qq.SortOrder))
                        .ThenInclude(qq => qq.Options.OrderBy(o => o.SortOrder))
                    .Include(q => q.Questions)
                        .ThenInclude(qq => qq.DisplayConditions)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (questionnaire == null)
                {
                    return NotFound("未找到指定的问卷");
                }

                var model = new EditQuestionnaireModel
                {
                    Id = questionnaire.Id,
                    Name = questionnaire.Name,
                    DisplayName = questionnaire.DisplayName,
                    Description = questionnaire.Description,
                    MainPicture = questionnaire.MainPicture,
                    Thumbnail = questionnaire.Thumbnail,
                    BeginTime = questionnaire.BeginTime,
                    EndTime = questionnaire.EndTime,
                    IsActive = questionnaire.IsActive,
                    AllowMultipleSubmissions = questionnaire.AllowMultipleSubmissions,
                    RequireLogin = questionnaire.RequireLogin,
                    Priority = questionnaire.Priority,
                    ThankYouMessage = questionnaire.ThankYouMessage
                };

                // 映射题目
                foreach (var question in questionnaire.Questions)
                {
                    var questionModel = new EditQuestionnaireQuestionModel
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
                        questionModel.Options.Add(new EditQuestionOptionModel
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
                        questionModel.DisplayConditions.Add(new EditQuestionDisplayConditionModel
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

                    model.Questions.Add(questionModel);
                }

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取问卷编辑信息时发生错误，问卷ID: {QuestionnaireId}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 编辑问卷
        /// </summary>
        /// <param name="model">编辑模型</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditQuestionnaireAsync(EditQuestionnaireModel model)
        {
            try
            {
                var validationResult = model.Validate();
                if (!validationResult.Successful)
                {
                    return BadRequest(validationResult);
                }

                var questionnaire = await _questionnaireRepository.GetAll()
                    .FirstOrDefaultAsync(q => q.Id == model.Id);

                if (questionnaire == null)
                {
                    return NotFound("未找到指定的问卷");
                }

                // 检查名称唯一性
                var existingQuestionnaire = await _questionnaireRepository.GetAll()
                    .FirstOrDefaultAsync(q => q.Name == model.Name && q.Id != model.Id);

                if (existingQuestionnaire != null)
                {
                    return BadRequest(new Result { Successful = false, Error = "问卷名称已存在" });
                }

                // 更新问卷信息
                questionnaire.Name = model.Name;
                questionnaire.DisplayName = model.DisplayName;
                questionnaire.Description = model.Description;
                questionnaire.MainPicture = model.MainPicture;
                questionnaire.Thumbnail = model.Thumbnail;
                questionnaire.BeginTime = model.BeginTime;
                questionnaire.EndTime = model.EndTime;
                questionnaire.IsActive = model.IsActive;
                questionnaire.AllowMultipleSubmissions = model.AllowMultipleSubmissions;
                questionnaire.RequireLogin = model.RequireLogin;
                questionnaire.Priority = model.Priority;
                questionnaire.ThankYouMessage = model.ThankYouMessage;
                questionnaire.LastEditTime = DateTime.Now.ToCstTime();

                await _questionnaireRepository.UpdateAsync(questionnaire);

                // 更新题目
                await CreateOrUpdateQuestions(questionnaire.Id, model.Questions);

                return Ok(new Result { Successful = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "编辑问卷时发生错误");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 隐藏/显示问卷
        /// </summary>
        /// <param name="model">隐藏模型</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenQuestionnaireAsync(HiddenQuestionnaireModel model)
        {
            try
            {
                var questionnaire = await _questionnaireRepository.GetAll()
                    .FirstOrDefaultAsync(q => q.Id == model.Id);

                if (questionnaire == null)
                {
                    return NotFound("未找到指定的问卷");
                }

                questionnaire.IsHidden = model.IsHidden;
                await _questionnaireRepository.UpdateAsync(questionnaire);

                return Ok(new Result { Successful = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "隐藏/显示问卷时发生错误，问卷ID: {QuestionnaireId}", model.Id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 编辑问卷优先级
        /// </summary>
        /// <param name="model">优先级模型</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditQuestionnairePriorityAsync(EditQuestionnairePriorityViewModel model)
        {
            try
            {
                var questionnaires = await _questionnaireRepository.GetAll()
                    .Where(q => model.Ids.Contains(q.Id))
                    .ToListAsync();

                for (int i = 0; i < model.Ids.Length; i++)
                {
                    var questionnaire = questionnaires.FirstOrDefault(q => q.Id == model.Ids[i]);
                    if (questionnaire != null)
                    {
                        questionnaire.Priority = model.Ids.Length - i;
                        await _questionnaireRepository.UpdateAsync(questionnaire);
                    }
                }

                return Ok(new Result { Successful = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "编辑问卷优先级时发生错误");
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取问卷统计信息
        /// </summary>
        /// <param name="id">问卷ID</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionnaireStatisticsModel>> GetQuestionnaireStatisticsAsync(long id)
        {
            try
            {
                var statistics = await _questionnaireService.GetQuestionnaireStatisticsAsync(id);

                // 设置问卷名称
                var questionnaire = await _questionnaireRepository.GetAll()
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (questionnaire != null)
                {
                    statistics.QuestionnaireName = questionnaire.DisplayName;
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取问卷统计信息时发生错误，问卷ID: {QuestionnaireId}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取问卷用户回答列表
        /// </summary>
        /// <param name="id">问卷ID</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<UserResponseListModel>> GetUserResponseListAsync(long id)
        {
            try
            {
                var result = await _questionnaireService.GetUserResponseListAsync(id);

                if (result == null)
                {
                    return NotFound("未找到指定的问卷");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户回答列表时发生错误，问卷ID: {QuestionnaireId}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取用户回答详情
        /// </summary>
        /// <param name="id">回答ID</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<UserResponseDetailModel>> GetUserResponseDetailAsync(long id)
        {
            try
            {
                var result = await _questionnaireService.GetUserResponseDetailAsync(id);

                if (result == null)
                {
                    return NotFound("未找到指定的回答");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户回答详情时发生错误，回答ID: {ResponseId}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取问卷题目分析数据
        /// </summary>
        /// <param name="id">问卷ID</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<QuestionAnalysisModel>> GetQuestionAnalysisAsync(long id)
        {
            try
            {
                var result = await _questionnaireService.GetQuestionAnalysisAsync(id);

                if (result == null)
                {
                    return NotFound("未找到指定的问卷");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取题目分析数据时发生错误，问卷ID: {QuestionnaireId}", id);
                return StatusCode(500, "服务器内部错误");
            }
        }

        /// <summary>
        /// 获取问卷管理列表
        /// </summary>
        /// <param name="model">查询参数</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<QueryResultModel<QuestionnaireOverviewModel>>> ListAsync(QueryParameterModel model)
        {
            try
            {
                var (items, total) = await _queryService.QueryAsync<Questionnaire, long>(_questionnaireRepository.GetAll().AsNoTracking(), model,
                    s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.DisplayName.Contains(model.SearchText)));

                return Ok(new QueryResultModel<QuestionnaireOverviewModel>
                {
                    Items = await items.Select(q => new QuestionnaireOverviewModel
                    {
                        Id = q.Id,
                        Name = q.Name,
                        DisplayName = q.DisplayName,
                        CreateTime = q.CreateTime,
                        LastEditTime = q.LastEditTime,
                        BeginTime = q.BeginTime,
                        EndTime = q.EndTime,
                        IsActive = q.IsActive,
                        IsHidden = q.IsHidden,
                        ResponseCount = q.ResponseCount,
                        Priority = q.Priority,
                        Status = GetQuestionnaireStatus(q)
                    }).ToListAsync(),
                    Total = total,
                    Parameter = model
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取问卷管理列表时发生错误");
                return StatusCode(500, "服务器内部错误");
            }
        }

        #endregion

        #region 私有方法

        private async Task CreateOrUpdateQuestions(long questionnaireId, List<EditQuestionnaireQuestionModel> questionModels)
        {
            // 获取现有题目
            var existingQuestions = await _questionRepository.GetAll()
                .Include(q => q.Options)
                .Include(q => q.DisplayConditions)
                .Where(q => q.QuestionnaireId == questionnaireId)
                .ToListAsync();

            // 删除不再存在的题目
            var currentQuestionIds = questionModels.Where(q => q.Id > 0).Select(q => q.Id).ToList();
            var questionsToDelete = existingQuestions.Where(q => !currentQuestionIds.Contains(q.Id)).ToList();

            foreach (var question in questionsToDelete)
            {
                // 先删除与该题目相关的用户回答记录，避免外键约束错误
                var questionResponses = await _questionResponseRepository.GetAll()
                    .Where(qr => qr.QuestionId == question.Id)
                    .ToListAsync();

                foreach (var questionResponse in questionResponses)
                {
                    await _questionResponseRepository.DeleteAsync(questionResponse);
                }

                // 删除以该题目作为触发条件的显示条件
                var triggeredConditions = await _conditionRepository.GetAll()
                    .Where(c => c.TriggerQuestionId == question.Id)
                    .ToListAsync();

                foreach (var condition in triggeredConditions)
                {
                    await _conditionRepository.DeleteAsync(condition);
                }

                // 然后删除题目本身
                await _questionRepository.DeleteAsync(question);
            }

            // 创建或更新题目
            foreach (var questionModel in questionModels)
            {
                QuestionnaireQuestion question;

                if (questionModel.Id > 0)
                {
                    question = existingQuestions.FirstOrDefault(q => q.Id == questionModel.Id);
                    if (question == null) continue;
                }
                else
                {
                    question = new QuestionnaireQuestion { QuestionnaireId = questionnaireId };
                }

                // 更新题目信息
                question.Title = questionModel.Title;
                question.Description = questionModel.Description;
                question.QuestionType = questionModel.QuestionType;
                question.IsRequired = questionModel.IsRequired;
                question.SortOrder = questionModel.SortOrder;
                question.MinSelectionCount = questionModel.MinSelectionCount;
                question.MaxSelectionCount = questionModel.MaxSelectionCount;
                question.MaxTextLength = questionModel.MaxTextLength;
                question.TextPlaceholder = questionModel.TextPlaceholder;

                if (questionModel.Id > 0)
                {
                    await _questionRepository.UpdateAsync(question);
                }
                else
                {
                    question = await _questionRepository.InsertAsync(question);
                }

                // 处理选项
                await CreateOrUpdateOptions(question.Id, questionModel.Options);

                // 处理显示条件
                await CreateOrUpdateDisplayConditions(question.Id, questionModel.DisplayConditions);
            }
        }

        private async Task CreateOrUpdateOptions(long questionId, List<EditQuestionOptionModel> optionModels)
        {
            // 获取现有选项
            var existingOptions = await _optionRepository.GetAll()
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();

            // 删除不再存在的选项
            var currentOptionIds = optionModels.Where(o => o.Id > 0).Select(o => o.Id).ToList();
            var optionsToDelete = existingOptions.Where(o => !currentOptionIds.Contains(o.Id)).ToList();

            foreach (var option in optionsToDelete)
            {
                // 先删除引用该选项的显示条件，避免外键约束错误
                var optionConditions = await _conditionRepository.GetAll()
                    .Where(c => c.TriggerOptionId == option.Id)
                    .ToListAsync();

                foreach (var condition in optionConditions)
                {
                    await _conditionRepository.DeleteAsync(condition);
                }

                // 然后删除选项本身
                await _optionRepository.DeleteAsync(option);
            }

            // 创建或更新选项
            foreach (var optionModel in optionModels)
            {
                QuestionOption option;

                if (optionModel.Id > 0)
                {
                    option = existingOptions.FirstOrDefault(o => o.Id == optionModel.Id);
                    if (option == null) continue;
                }
                else
                {
                    option = new QuestionOption { QuestionId = questionId };
                }

                option.Text = optionModel.Text;
                option.Value = optionModel.Value;
                option.SortOrder = optionModel.SortOrder;
                option.IsEnabled = optionModel.IsEnabled;
                option.Image = optionModel.Image;

                if (optionModel.Id > 0)
                {
                    await _optionRepository.UpdateAsync(option);
                }
                else
                {
                    await _optionRepository.InsertAsync(option);
                }
            }
        }

        private async Task CreateOrUpdateDisplayConditions(long questionId, List<EditQuestionDisplayConditionModel> conditionModels)
        {
            // 获取现有条件
            var existingConditions = await _conditionRepository.GetAll()
                .Where(c => c.ControlledQuestionId == questionId)
                .ToListAsync();

            // 删除不再存在的条件
            var currentConditionIds = conditionModels.Where(c => c.Id > 0).Select(c => c.Id).ToList();
            var conditionsToDelete = existingConditions.Where(c => !currentConditionIds.Contains(c.Id)).ToList();

            foreach (var condition in conditionsToDelete)
            {
                await _conditionRepository.DeleteAsync(condition);
            }

            // 创建或更新条件
            foreach (var conditionModel in conditionModels)
            {
                QuestionDisplayCondition condition;

                if (conditionModel.Id > 0)
                {
                    condition = existingConditions.FirstOrDefault(c => c.Id == conditionModel.Id);
                    if (condition == null) continue;
                }
                else
                {
                    condition = new QuestionDisplayCondition { ControlledQuestionId = questionId };
                }

                if (conditionModel.TriggerQuestionId == null)
                {
                    throw new Exception("触发题目ID不能为空");
                }

                condition.ConditionType = conditionModel.ConditionType;
                condition.LogicalOperator = conditionModel.LogicalOperator;
                condition.TriggerQuestionId = conditionModel.TriggerQuestionId.Value;
                condition.TriggerOptionId = conditionModel.TriggerOptionId;
                condition.ExpectedValue = conditionModel.ExpectedValue;
                condition.ConditionGroup = conditionModel.ConditionGroup;

                if (conditionModel.Id > 0)
                {
                    await _conditionRepository.UpdateAsync(condition);
                }
                else
                {
                    await _conditionRepository.InsertAsync(condition);
                }
            }
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

        #endregion
    }
}
