using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.SpaceEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ISpaceCommandService
{
    /// <summary>
    /// 获取当前用户的编辑数据（个人资料 + 个人主页）。
    /// </summary>
    Task<SdkResult<SpaceEditViewModel>> GetSpaceEditAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 提交个人资料和个人主页的编辑。
    /// </summary>
    Task<SdkResult<string>> SubmitEditAsync(SpaceEditViewModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刷新当前用户的 Steam 游玩记录。
    /// </summary>
    Task<SdkResult<string>> RefreshSteamInfoAsync(string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将当前用户所有消息标记为已读。
    /// </summary>
    Task<SdkResult<string>> ReadAllMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行每日签到。
    /// </summary>
    Task<SdkResult<string>> SignInAsync(CancellationToken cancellationToken = default);
}
