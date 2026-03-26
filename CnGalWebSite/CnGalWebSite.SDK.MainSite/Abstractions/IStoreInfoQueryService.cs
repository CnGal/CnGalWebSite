using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IStoreInfoQueryService
{
    /// <summary>
    /// 获取所有在售游戏的商店折扣信息
    /// </summary>
    Task<SdkResult<IReadOnlyList<DiscountGameItem>>> GetAllGameStoreInfoAsync(CancellationToken cancellationToken = default);
}
