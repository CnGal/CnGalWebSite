using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ISpaceQueryService
{
    Task<SdkResult<SpaceDetailViewModel>> GetSpaceDetailAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前登录用户的全部消息，按类型分组返回。
    /// </summary>
    Task<SdkResult<MessageListViewModel>> GetUserMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前用户未读消息数量。
    /// </summary>
    Task<SdkResult<long>> GetUnreadMessageCountAsync(CancellationToken cancellationToken = default);
}
