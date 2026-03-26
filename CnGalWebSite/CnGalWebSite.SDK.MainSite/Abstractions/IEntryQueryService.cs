using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IEntryQueryService
{
    Task<SdkResult<EntryDetailViewModel>> GetEntryDetailAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定年月的游戏发布列表（卡片模式）
    /// </summary>
    Task<SdkResult<IReadOnlyList<GamePublishTimesCardItem>>> GetPublishGamesByTimeAsync(
        int year, int month, int mode = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定日期范围的游戏发布时间线
    /// </summary>
    Task<SdkResult<IReadOnlyList<GamePublishTimesTimelineItem>>> GetPublishGamesTimelineAsync(
        long afterTime, long beforeTime, int mode = 0, CancellationToken cancellationToken = default);
}
