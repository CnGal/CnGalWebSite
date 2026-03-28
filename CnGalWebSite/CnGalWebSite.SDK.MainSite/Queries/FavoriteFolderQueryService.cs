using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class FavoriteFolderQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<FavoriteFolderQueryService> logger) : QueryServiceBase(httpClient), IFavoriteFolderQueryService
{
    private static readonly TimeSpan FolderDetailCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan UserFoldersCacheDuration = TimeSpan.FromMinutes(5);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<FavoriteFolderDetailViewModel>> GetFavoriteFolderDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:favorite-folder-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out FavoriteFolderDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<FavoriteFolderDetailViewModel>.Ok(cached);
        }

        var path = $"api/favorites/GetView/{id}";
        var result = await GetSingleAsync<FavoriteFolderViewModel, FavoriteFolderDetailViewModel>(
            path,
            MapToDetailViewModel,
            "FAVORITE_FOLDER",
            "收藏夹",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, FolderDetailCacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<IReadOnlyList<FavoriteFolderSummaryItem>>> GetUserFavoriteFoldersAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:user-favorite-folders:{userId}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<FavoriteFolderSummaryItem>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<FavoriteFolderSummaryItem>>.Ok(cached);
        }

        var path = $"api/favorites/GetUserFavoriteFolders/{userId}";
        var result = await GetAsync<IReadOnlyList<FavoriteFolderSummaryItem>>(
            path,
            "FAVORITE_FOLDER",
            "用户收藏夹列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, UserFoldersCacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<EditFavoriteFolderViewModel>> GetFavoriteFolderForEditAsync(long id, CancellationToken cancellationToken = default)
    {
        var path = $"api/favorites/EditFavoriteFolder/{id}";
        return await GetAsync<EditFavoriteFolderViewModel>(
            path,
            "FAVORITE_FOLDER",
            "收藏夹编辑数据",
            cancellationToken);
    }

    public async Task<SdkResult<IReadOnlyList<FavoriteObjectOverviewModel>>> GetFavoriteObjectsAsync(long folderId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var path = $"api/favorites/ListUserFavoriteObjects?folderId={folderId}";
        var queryParam = new CnGalWebSite.Core.Models.QueryParameterModel
        {
            Page = page,
            ItemsPerPage = pageSize,
        };

        try
        {
            var response = await HttpClient.PostAsJsonAsync(path, queryParam, SdkJsonSerializerOptions.Default, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "获取收藏对象列表失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));

                return SdkResult<IReadOnlyList<FavoriteObjectOverviewModel>>.Fail("FAVORITE_FOLDER_OBJECTS_HTTP_FAILED", $"获取收藏对象列表失败（HTTP {(int)response.StatusCode})");
            }

            var queryResult = Deserialize<CnGalWebSite.Core.Models.QueryResultModel<FavoriteObjectOverviewModel>>(responseBody);
            IReadOnlyList<FavoriteObjectOverviewModel> data = queryResult?.Items?.ToList() ?? [];
            return SdkResult<IReadOnlyList<FavoriteObjectOverviewModel>>.Ok(data);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取收藏对象列表异常。Path={Path}; BaseAddress={BaseAddress}", path, HttpClient.BaseAddress);
            return SdkResult<IReadOnlyList<FavoriteObjectOverviewModel>>.Fail("FAVORITE_FOLDER_OBJECTS_EXCEPTION", "请求收藏对象列表时发生异常");
        }
    }

    private static FavoriteFolderDetailViewModel MapToDetailViewModel(FavoriteFolderViewModel dto)
    {
        var objects = dto.Objects ?? [];
        var userInfo = dto.UserInfor ?? new UserInforViewModel();

        return new FavoriteFolderDetailViewModel
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            BriefIntroduction = dto.BriefIntroduction ?? string.Empty,
            MainPicture = dto.MainPicture ?? string.Empty,
            CreateTime = dto.CreateTime,
            LastEditTime = dto.LastEditTime,
            ReaderCount = dto.ReaderCount,
            IsHidden = dto.IsHidden,
            Authority = dto.Authority,
            UserName = userInfo.Name ?? string.Empty,
            UserPhotoPath = userInfo.PhotoPath ?? string.Empty,
            UserId = userInfo.Id ?? string.Empty,
            Entries = objects.Where(o => o.entry != null).Select(o => o.entry!).ToList(),
            Articles = objects.Where(o => o.article != null).Select(o => o.article!).ToList(),
            Videos = objects.Where(o => o.Video != null).Select(o => o.Video!).ToList(),
            Tags = objects.Where(o => o.Tag != null).Select(o => o.Tag!).ToList(),
            Peripheries = objects.Where(o => o.periphery != null).Select(o => o.periphery!).ToList(),
        };
    }
}
