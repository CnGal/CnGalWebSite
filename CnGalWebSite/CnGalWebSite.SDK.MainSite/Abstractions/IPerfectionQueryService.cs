using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IPerfectionQueryService
{
    /// <summary>
    /// 获取全站完善度概览统计数据（各等级数量、平均值、中位数、众数、标准差）。
    /// </summary>
    Task<SdkResult<PerfectionLevelOverviewModel>> GetPerfectionLevelOverviewAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定完善度等级的随机词条列表。
    /// </summary>
    Task<SdkResult<IReadOnlyList<PerfectionInforTipViewModel>>> GetPerfectionLevelRadomListAsync(
        PerfectionLevel level,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定检查优先级的随机检查项列表。
    /// </summary>
    Task<SdkResult<IReadOnlyList<PerfectionCheckViewModel>>> GetPerfectionCheckLevelRadomListAsync(
        PerfectionCheckLevel level,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取完善度/编辑趋势折线图数据。
    /// </summary>
    Task<SdkResult<LineChartModel>> GetPerfectionLineChartAsync(
        LineChartType type,
        DateTime afterTime,
        DateTime beforeTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取最近编辑列表。
    /// </summary>
    Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetRecentlyEditListAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询完善度列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<PerfectionOverviewModel>>> ListPerfectionsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询完善度单项检查列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>> ListPerfectionChecksAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);
}
