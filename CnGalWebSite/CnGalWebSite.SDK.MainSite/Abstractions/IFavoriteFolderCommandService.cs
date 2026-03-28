using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IFavoriteFolderCommandService
{
    /// <summary>
    /// 编辑收藏夹基本信息
    /// </summary>
    Task<SdkResult<bool>> EditFavoriteFolderAsync(EditFavoriteFolderViewModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建收藏夹
    /// </summary>
    Task<SdkResult<bool>> CreateFavoriteFolderAsync(CreateFavoriteFolderViewModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除收藏夹中的收藏对象
    /// </summary>
    Task<SdkResult<bool>> DeleteFavoriteObjectsAsync(long folderId, long[] objectIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除收藏夹
    /// </summary>
    Task<SdkResult<bool>> DeleteFavoriteFolderAsync(long[] folderIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移动收藏对象到其他收藏夹
    /// </summary>
    Task<SdkResult<bool>> MoveFavoriteObjectsAsync(MoveFavoriteObjectsModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加对象到默认收藏夹
    /// </summary>
    Task<SdkResult<bool>> AddFavoriteObjectAsync(long objectId, FavoriteObjectType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从所有收藏夹中取消收藏某个对象
    /// </summary>
    Task<SdkResult<bool>> UnFavoriteObjectAsync(long objectId, FavoriteObjectType type, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查对象是否被当前用户收藏
    /// </summary>
    Task<SdkResult<bool>> IsObjectFavoritedAsync(long objectId, FavoriteObjectType type, CancellationToken cancellationToken = default);
}
