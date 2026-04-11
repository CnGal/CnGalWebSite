using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IExamineQueryService
{
    /// <summary>
    /// 分页查询审核列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<ExamineOverviewModel>>> GetExaminesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询用户审阅记录列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<UserReviewEditRecordOverviewModel>>> GetUserReviewEditRecordsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询用户监控列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<UserMonitorOverviewModel>>> GetUserMonitorsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户内容中心概览数据。
    /// </summary>
    Task<SdkResult<UserContentCenterViewModel>> GetUserContentCenterAsync(
        CancellationToken cancellationToken = default);
}
