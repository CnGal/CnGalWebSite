using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class FavoriteFolderCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IHttpContextAccessor httpContextAccessor,
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
        memoryCache.Remove($"main-site:space-detail:{userId}");
    }

    private string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
    }
}
