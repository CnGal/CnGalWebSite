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
}
