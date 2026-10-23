using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 审核查询。调用方取消时抛出 OperationCanceledException；HTTP 超时仍返回失败结果。
/// 身份变化时最多重试一次，再次变化返回 EXAMINE_AUTH_CONTEXT_CHANGED。
/// </summary>
public interface IExamineQueryService
{
    /// <summary>
    /// 分页查询审核列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<ExamineOverviewModel, ExamineSortField>>> GetExaminesAsync(
        QueryParameterModel<ExamineSortField> parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询用户审阅记录列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<UserReviewEditRecordOverviewModel, UserReviewEditRecordSortField>>> GetUserReviewEditRecordsAsync(
        QueryParameterModel<UserReviewEditRecordSortField> parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询用户监控列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<UserMonitorOverviewModel, UserMonitorSortField>>> GetUserMonitorsAsync(
        QueryParameterModel<UserMonitorSortField> parameter,
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

    /// <summary>
    /// 获取指定词条的编辑记录列表（已通过的审核记录）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetEntryEditRecordsAsync(
        int entryId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定文章的编辑记录列表（已通过的审核记录）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetArticleEditRecordsAsync(
        long articleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定视频的编辑记录列表（已通过的审核记录）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetVideoEditRecordsAsync(
        long videoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定标签的编辑记录列表（已通过的审核记录）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetTagEditRecordsAsync(
        long tagId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定周边的编辑记录列表（已通过的审核记录）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetPeripheryEditRecordsAsync(
        long peripheryId,
        CancellationToken cancellationToken = default);
}
