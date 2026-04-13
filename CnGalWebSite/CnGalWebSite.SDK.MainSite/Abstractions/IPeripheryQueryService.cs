using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IPeripheryQueryService
{
    Task<SdkResult<PeripheryDetailViewModel>> GetPeripheryDetailAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取词条关联的周边概览（用于词条详情页周边卡片）
    /// </summary>
    Task<SdkResult<GameOverviewPeripheryListModel>> GetEntryOverviewPeripheriesAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户拥有的周边列表（按关联词条分组）。
    /// </summary>
    Task<SdkResult<IReadOnlyList<GameOverviewPeripheryListModel>>> GetUserOverviewPeripheriesAsync(string userId, CancellationToken cancellationToken = default);
}

