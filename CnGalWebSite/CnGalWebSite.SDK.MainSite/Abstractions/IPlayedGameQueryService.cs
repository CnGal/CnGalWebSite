using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IPlayedGameQueryService
{
    /// <summary>
    /// 获取词条的评分概览数据。
    /// </summary>
    Task<SdkResult<PlayedGameOverviewViewModel>> GetOverviewAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的游玩记录列表。
    /// </summary>
    Task<SdkResult<UserGameRecordsViewModel>> GetUserGameRecordsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的 Steam 账号信息列表。
    /// </summary>
    Task<SdkResult<IReadOnlyList<SteamUserInfoItem>>> GetUserSteamInfoAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 Steam 游戏总览（库存排行 + 拥有率排行）。
    /// </summary>
    Task<SdkResult<SteamGamesOverviewViewModel>> GetSteamGamesOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取随机用户评价列表。
    /// </summary>
    Task<SdkResult<IReadOnlyList<RandomReviewItem>>> GetRandomReviewsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除指定用户的游玩记录缓存。
    /// </summary>
    void InvalidateUserRecordsCache(string userId);

    /// <summary>
    /// 清除指定词条的评分概览缓存。
    /// </summary>
    void InvalidateOverviewCache(int entryId);
}
