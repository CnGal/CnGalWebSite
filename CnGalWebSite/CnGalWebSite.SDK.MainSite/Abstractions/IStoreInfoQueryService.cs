using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.DataModel.ViewModel.Stores;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IStoreInfoQueryService
{
    /// <summary>
    /// 获取所有在售游戏的商店折扣信息
    /// </summary>
    Task<SdkResult<IReadOnlyList<DiscountGameItem>>> GetAllGameStoreInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取按年份分组的游戏销售额信息
    /// </summary>
    Task<SdkResult<GameRevenueInfoViewModel>> GetGameRevenueInfoAsync(int year, int page, int max, int order, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 "CnGal 世代" 历年热门国产 Galgame 列表（按年份分组，每年至多 12 款）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<CnGalGenerationYearItem>>> GetCnGalGenerationAsync(CancellationToken cancellationToken = default);
}
