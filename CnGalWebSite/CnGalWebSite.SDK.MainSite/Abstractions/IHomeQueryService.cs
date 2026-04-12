using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IHomeQueryService
{
    Task<SdkResult<HomeSummaryViewModel>> GetHomeSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 搜索词条、文章、标签、视频、周边等内容。
    /// </summary>
    /// <param name="queryString">完整的查询字符串（以 ? 开头），由 SearchInputModel.ToQueryParameterString() 生成</param>
    Task<SdkResult<SearchViewModel>> SearchAsync(string queryString, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取广场页聚合数据（随机标签、抽奖列表、投票列表）。
    /// </summary>
    Task<SdkResult<SquareSummaryViewModel>> GetSquareSummaryAsync(CancellationToken cancellationToken = default);
}
