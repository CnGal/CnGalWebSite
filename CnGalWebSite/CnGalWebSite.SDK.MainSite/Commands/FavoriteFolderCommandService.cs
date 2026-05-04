using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class FavoriteFolderCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ICurrentUserAccessor currentUserAccessor,
    ILogger<FavoriteFolderCommandService> logger) : CommandServiceBase(httpClient), IFavoriteFolderCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<bool>> EditFavoriteFolderAsync(EditFavoriteFolderViewModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result =
                await PostAsJsonAsync<EditFavoriteFolderViewModel, Result>("api/favorites/EditFavoriteFolder",
                    model, cancellationToken);
            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_FOLDER_EDIT_EMPTY_RESPONSE", "编辑收藏夹返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_FOLDER_EDIT_FAILED", result.Error ?? "编辑收藏夹失败");
            }

            InvalidateFolderCaches(model.Id);
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "编辑收藏夹异常。FolderId={FolderId}; BaseAddress={BaseAddress}", model.Id,
                HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_FOLDER_EDIT_EXCEPTION", "编辑收藏夹时发生异常");
        }
    }

    public async Task<SdkResult<bool>> CreateFavoriteFolderAsync(CreateFavoriteFolderViewModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result =
                await PostAsJsonAsync<CreateFavoriteFolderViewModel, Result>("api/favorites/CreateFavoriteFolder",
                    model, cancellationToken);
            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_FOLDER_CREATE_EMPTY_RESPONSE", "创建收藏夹返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_FOLDER_CREATE_FAILED", result.Error ?? "创建收藏夹失败");
            }

            InvalidateUserFolderListCache();
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "创建收藏夹异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_FOLDER_CREATE_EXCEPTION", "创建收藏夹时发生异常");
        }
    }

    public async Task<SdkResult<bool>> DeleteFavoriteObjectsAsync(long folderId, long[] objectIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<DeleteFavoriteObjectsModel, Result>(
                "api/favorites/UserDeleteFavoriteObject",
                new DeleteFavoriteObjectsModel { FavorieFolderId = folderId, Ids = objectIds },
                cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_OBJECT_DELETE_EMPTY_RESPONSE", "删除收藏对象返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_OBJECT_DELETE_FAILED", result.Error ?? "删除收藏对象失败");
            }

            InvalidateFolderCaches(folderId);
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "删除收藏对象异常。FolderId={FolderId}; BaseAddress={BaseAddress}", folderId,
                HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_OBJECT_DELETE_EXCEPTION", "删除收藏对象时发生异常");
        }
    }

    public async Task<SdkResult<bool>> DeleteFavoriteFolderAsync(long[] folderIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<DeleteFavoriteFoldersModel, Result>(
                "api/favorites/UserDeleteFavoriteFolderAsync",
                new DeleteFavoriteFoldersModel { Ids = folderIds },
                cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_FOLDER_DELETE_EMPTY_RESPONSE", "删除收藏夹返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_FOLDER_DELETE_FAILED", result.Error ?? "删除收藏夹失败");
            }

            foreach (var folderId in folderIds)
            {
                InvalidateFolderCaches(folderId);
            }

            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "删除收藏夹异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_FOLDER_DELETE_EXCEPTION", "删除收藏夹时发生异常");
        }
    }

    public async Task<SdkResult<bool>> MoveFavoriteObjectsAsync(MoveFavoriteObjectsModel model,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<MoveFavoriteObjectsModel, Result>(
                "api/favorites/MoveFavoriteObjects",
                model,
                cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_OBJECT_MOVE_EMPTY_RESPONSE", "移动收藏对象返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_OBJECT_MOVE_FAILED", result.Error ?? "移动收藏对象失败");
            }

            // Invalidate source folder cache
            InvalidateFolderCaches(model.CurrentFolderId);
            // Invalidate target folder caches
            foreach (var folderId in model.FolderIds)
            {
                InvalidateFolderCaches(folderId);
            }

            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "移动收藏对象异常。CurrentFolderId={FolderId}; BaseAddress={BaseAddress}", model.CurrentFolderId,
                HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_OBJECT_MOVE_EXCEPTION", "移动收藏对象时发生异常");
        }
    }

    /// <summary>
    /// 清空指定收藏夹的详情缓存，以及当前用户的收藏夹列表缓存和空间详情缓存
    /// </summary>
    private void InvalidateFolderCaches(long folderId)
    {
        // 清空收藏夹详情缓存
        memoryCache.Remove($"main-site:favorite-folder-detail:{folderId}");

        // 清空当前用户的收藏夹列表缓存与空间详情缓存
        InvalidateUserFolderListCache();
    }

    /// <summary>
    /// 清空当前用户的收藏夹列表缓存和空间详情缓存
    /// </summary>
    private void InvalidateUserFolderListCache()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        memoryCache.Remove($"main-site:user-favorite-folders:{userId}");
        memoryCache.Remove($"main-site:user-all-favorite-folders:{userId}");
        memoryCache.Remove($"main-site:space-detail:{userId}");
    }

    private string? GetCurrentUserId()
    {
        return currentUserAccessor.GetCurrentUserId();
    }

    public async Task<SdkResult<bool>> AddFavoriteObjectAsync(long objectId, FavoriteObjectType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, get the user's default folder ids
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return SdkResult<bool>.Fail("FAVORITE_NOT_AUTHENTICATED", "未登录，无法收藏");
            }

            var folders = await GetFromJsonAsync<List<FavoriteFolderOverviewModel>>(
                $"api/favorites/GetUserFavoriteFolders/{userId}", cancellationToken);

            if (folders is null || folders.Count == 0)
            {
                return SdkResult<bool>.Fail("FAVORITE_NO_FOLDERS", "无法获取收藏夹信息");
            }

            var defaultFolderIds = folders.Where(f => f.IsDefault).Select(f => f.Id).ToArray();
            if (defaultFolderIds.Length == 0)
            {
                // If no default folder, use the first one
                defaultFolderIds = [folders[0].Id];
            }

            var result = await PostAsJsonAsync<AddFavoriteObjectViewModel, Result>(
                "api/favorites/AddFavoriteObject",
                new AddFavoriteObjectViewModel { FavoriteFolderIds = defaultFolderIds, ObjectId = objectId, Type = type },
                cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_ADD_EMPTY_RESPONSE", "收藏返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_ADD_FAILED", result.Error ?? "收藏失败");
            }

            InvalidateUserFolderListCache();
            foreach (var folderId in defaultFolderIds)
            {
                memoryCache.Remove($"main-site:favorite-folder-detail:{folderId}");
            }
            memoryCache.Remove($"main-site:relate-favorite-folders:{(int)type}:{objectId}");
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "收藏对象异常。ObjectId={ObjectId}; Type={Type}; BaseAddress={BaseAddress}",
                objectId, type, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_ADD_EXCEPTION", "收藏时发生异常");
        }
    }

    public async Task<SdkResult<bool>> UnFavoriteObjectAsync(long objectId, FavoriteObjectType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<UnFavoriteObjectsModel, Result>(
                "api/favorites/UnFavoriteObjects",
                new UnFavoriteObjectsModel { ObjectId = objectId, Type = type },
                cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Fail("FAVORITE_REMOVE_EMPTY_RESPONSE", "取消收藏返回空响应");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("FAVORITE_REMOVE_FAILED", result.Error ?? "取消收藏失败");
            }

            InvalidateUserFolderListCache();
            memoryCache.Remove($"main-site:relate-favorite-folders:{(int)type}:{objectId}");
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "取消收藏异常。ObjectId={ObjectId}; Type={Type}; BaseAddress={BaseAddress}",
                objectId, type, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("FAVORITE_REMOVE_EXCEPTION", "取消收藏时发生异常");
        }
    }

    public async Task<SdkResult<bool>> IsObjectFavoritedAsync(long objectId, FavoriteObjectType type,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<IsObjectInUserFavoriteFolderResult>(
                $"api/favorites/IsObjectInUserFavoriteFolder/{objectId}/{type}", cancellationToken);

            if (result is null)
            {
                return SdkResult<bool>.Ok(false);
            }

            return SdkResult<bool>.Ok(result.Result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "检查收藏状态异常。ObjectId={ObjectId}; Type={Type}; BaseAddress={BaseAddress}",
                objectId, type, HttpClient.BaseAddress);
            return SdkResult<bool>.Ok(false); // Fail silently, default to not favorited
        }
    }
}
