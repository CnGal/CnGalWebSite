using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IHomeQueryService
{
    Task<SdkResult<HomeSummaryViewModel>> GetHomeSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索词条、文章、标签、视频、周边等内容。
    /// </summary>
    /// <param name="request">包含强类型排序字段和方向的搜索请求</param>
    Task<SdkResult<SearchViewModel>> SearchAsync(CnGalWebSite.DataModel.Application.Search.Dtos.SearchInputModel request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取广场页聚合数据（随机标签、抽奖列表、投票列表）。
    /// </summary>
    Task<SdkResult<SquareSummaryViewModel>> GetSquareSummaryAsync(CancellationToken cancellationToken = default);
}
