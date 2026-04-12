using CnGalWebSite.DataModel.ViewModel.Space;
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

    /// <summary>
    /// 获取用户发表的文章列表（分页）。
    /// </summary>
    Task<SdkResult<UserArticlesViewModel>> GetUserArticlesAsync(string userId, int currentPage = 1, int maxResultCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户发表的视频列表。
    /// </summary>
    Task<SdkResult<UserVideosViewModel>> GetUserVideosAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户编辑记录列表（分页）。
    /// </summary>
    Task<SdkResult<UserEditRecordsViewModel>> GetUserEditRecordsAsync(string userId, int currentPage = 1, int maxResultCount = 12, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户热力图数据。
    /// </summary>
    Task<SdkResult<UserHeatMapViewModel>> GetUserHeatMapAsync(string userId, UserHeatMapType type, DateTime? after = null, DateTime? before = null, CancellationToken cancellationToken = default);
}

