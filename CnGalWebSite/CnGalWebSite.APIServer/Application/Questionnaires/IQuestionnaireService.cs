using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Questionnaires;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Questionnaires
{
    public interface IQuestionnaireService
    {
        /// <summary>
        /// 获取问卷详情
        /// </summary>
        /// <param name="id">问卷ID</param>
        /// <param name="userId">用户ID（可选）</param>
        /// <returns></returns>
        Task<QuestionnaireViewModel> GetQuestionnaireDetailAsync(long id, string userId = null);

        /// <summary>
        /// 获取问卷列表
        /// </summary>
        /// <param name="userId">用户ID（可选）</param>
        /// <returns></returns>
        Task<List<QuestionnaireCardViewModel>> GetQuestionnaireCardsAsync(string userId = null);

        /// <summary>
        /// 提交问卷
        /// </summary>
        /// <param name="model">提交模型</param>
        /// <param name="userId">用户ID</param>
        /// <param name="ipAddress">IP地址</param>
        /// <param name="userAgent">用户代理</param>
        /// <returns></returns>
        Task<Result> SubmitQuestionnaireAsync(SubmitQuestionnaireModel model, string userId, string ipAddress, string userAgent);

        /// <summary>
        /// 保存草稿
        /// </summary>
        /// <param name="model">草稿模型</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<Result> SaveDraftAsync(SaveDraftModel model, string userId);

        /// <summary>
        /// 验证显示条件
        /// </summary>
        /// <param name="conditions">显示条件</param>
        /// <param name="responses">用户回答</param>
        /// <returns></returns>
        bool EvaluateDisplayConditions(List<QuestionDisplayCondition> conditions, List<QuestionResponse> responses);

        /// <summary>
        /// 获取问卷统计信息
        /// </summary>
        /// <param name="questionnaireId">问卷ID</param>
        /// <returns></returns>
        Task<QuestionnaireStatisticsModel> GetQuestionnaireStatisticsAsync(long questionnaireId);

        /// <summary>
        /// 检查用户是否已提交问卷
        /// </summary>
        /// <param name="questionnaireId">问卷ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<bool> HasUserSubmittedAsync(long questionnaireId, string userId);

        /// <summary>
        /// 获取用户的问卷回答
        /// </summary>
        /// <param name="questionnaireId">问卷ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<QuestionnaireResponseViewModel> GetUserResponseAsync(long questionnaireId, string userId);
    }
}
