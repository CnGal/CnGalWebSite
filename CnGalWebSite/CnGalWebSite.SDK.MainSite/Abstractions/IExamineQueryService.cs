using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Home;
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

    /// <summary>
    /// 获取编辑记录概览（某条目/文章/标签/视频/周边的全部已通过审核记录）。
    /// </summary>
    Task<SdkResult<ExaminesOverviewViewModel>> GetExaminesOverviewAsync(
        long examineId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取单条审核记录详情。
    /// </summary>
    Task<SdkResult<ExamineViewModel>> GetExamineDetailAsync(
        long examineId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取两个版本之间的对比审核详情。
    /// 通过加载 contrastId 对应的审核记录，获取 BeforeModel（对比版本截止状态）和 AfterModel（当前版本截止状态）。
    /// </summary>
    Task<SdkResult<ExamineViewModel>> GetContrastExamineAsync(
        long contrastId,
        long currentId,
        CancellationToken cancellationToken = default);
}
