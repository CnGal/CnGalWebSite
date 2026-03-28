using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IFavoriteFolderQueryService
{
    /// <summary>
    /// 获取收藏夹详情（公开页面）
    /// </summary>
    Task<SdkResult<FavoriteFolderDetailViewModel>> GetFavoriteFolderDetailAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户的收藏夹列表
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteFolderSummaryItem>>> GetUserFavoriteFoldersAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取收藏夹编辑数据
    /// </summary>
    Task<SdkResult<EditFavoriteFolderViewModel>> GetFavoriteFolderForEditAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取收藏夹中的收藏对象列表（分页）
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteObjectOverviewModel>>> GetFavoriteObjectsAsync(long folderId, int page, int pageSize, CancellationToken cancellationToken = default);
}
