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
}
