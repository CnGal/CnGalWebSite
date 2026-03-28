using CnGalWebSite.DataModel.Model;
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
    /// 获取用户的收藏夹列表（公开 API，过滤隐藏的收藏夹）
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteFolderSummaryItem>>> GetUserFavoriteFoldersAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取当前用户的全部收藏夹列表（包含隐藏的收藏夹，仅本人/管理员可用）
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteFolderSummaryItem>>> GetAllUserFavoriteFoldersAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取收藏夹编辑数据
    /// </summary>
    Task<SdkResult<EditFavoriteFolderViewModel>> GetFavoriteFolderForEditAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取收藏夹中的收藏对象列表（分页）
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteObjectOverviewModel>>> GetFavoriteObjectsAsync(long folderId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// 通过收藏夹 Id 获取该用户的所有收藏夹列表（用于移动收藏对象时选择目标）
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteFolderOverviewModel>>> GetUserFoldersByFolderIdAsync(long folderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取包含指定对象的公开收藏夹列表（用于词条/文章等详情页的"相关目录"卡片）
    /// </summary>
    Task<SdkResult<IReadOnlyList<FavoriteFolderOverviewModel>>> GetRelateFavoriteFoldersAsync(FavoriteObjectType type, long id, CancellationToken cancellationToken = default);
}
