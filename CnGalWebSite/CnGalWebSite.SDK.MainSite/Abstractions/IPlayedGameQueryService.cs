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
}
