using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tables;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 数据汇总查询服务接口。
/// </summary>
public interface ITableQueryService
{
    /// <summary>
    /// 获取数据汇总概要（词条数、文章数、最后编辑时间等）。
    /// </summary>
    Task<SdkResult<TableViewModel>> GetTableSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询游戏列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<BasicInforTableModel>>> QueryGamesAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询制作组列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<GroupInforTableModel>>> QueryGroupsAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询 STAFF 列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<StaffInforTableModel>>> QueryStaffsAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询制作人列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<MakerInforTableModel>>> QueryMakersAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询角色列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<RoleInforTableModel>>> QueryRolesAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询游戏商店信息列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<StoreInfoViewModel>>> QueryStoreInfoAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 服务端分页查询游戏评分列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<GameScoreTableModel>>> QueryGameScoresAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default);
}
