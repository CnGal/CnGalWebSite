using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class PeripheryQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<PeripheryQueryService> logger) : QueryServiceBase(httpClient), IPeripheryQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<PeripheryDetailViewModel>> GetPeripheryDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = PublicCacheKeys.Detail("periphery", id);
        if (memoryCache.TryGetValue(cacheKey, out PeripheryDetailViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<PeripheryDetailViewModel>(cached, cacheRead);
        }

        var result = await GetSingleAsync<PeripheryViewModel, PeripheryDetailViewModel>(
            $"api/peripheries/GetPeripheryView/{id}",
            MapToViewModel,
            "PERIPHERY",
            "周边",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<GameOverviewPeripheryListModel>> GetEntryOverviewPeripheriesAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:entry-periphery-overview:{entryId}";
        if (memoryCache.TryGetValue(cacheKey, out GameOverviewPeripheryListModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<GameOverviewPeripheryListModel>(cached, cacheRead);
        }

        var path = $"api/peripheries/GetEntryOverviewPeripheries/{entryId}";
        var result = await GetAsync<GameOverviewPeripheryListModel>(
            path,
            "PERIPHERY",
            "词条周边概览",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<GameOverviewPeripheryListModel>>> GetUserOverviewPeripheriesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var path = $"api/peripheries/GetUserOverviewPeripheries/{userId}";
        var result = await GetAsync<List<GameOverviewPeripheryListModel>>(
            path,
            "PERIPHERY",
            "用户周边列表",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<GameOverviewPeripheryListModel>>.Fail(
                result.Error?.Code ?? "PERIPHERY_USER_OVERVIEW_FAILED",
                result.Error?.Message ?? "获取用户周边列表失败",
                result.Error?.StatusCode);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<IReadOnlyList<GameOverviewPeripheryListModel>>(result.Data, cacheRead);
    }

    private static PeripheryDetailViewModel MapToViewModel(PeripheryViewModel dto)
    {
        return new PeripheryDetailViewModel
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            DisplayName = dto.DisplayName ?? string.Empty,
            BriefIntroduction = dto.BriefIntroduction ?? string.Empty,
            MainPicture = dto.MainPicture ?? string.Empty,
            BackgroundPicture = dto.BackgroundPicture ?? string.Empty,
            SmallBackgroundPicture = dto.SmallBackgroundPicture ?? string.Empty,
            IsHidden = dto.IsHidden,
            Type = dto.Type,
            Category = dto.Category ?? string.Empty,
            Brand = dto.Brand ?? string.Empty,
            Author = dto.Author ?? string.Empty,
            Material = dto.Material ?? string.Empty,
            Size = dto.Size ?? string.Empty,
            IndividualParts = dto.IndividualParts ?? string.Empty,
            Price = dto.Price ?? string.Empty,
            SaleLink = dto.SaleLink ?? string.Empty,
            IsReprint = dto.IsReprint,
            IsAvailableItem = dto.IsAvailableItem,
            PageCount = dto.PageCount,
            SongCount = dto.SongCount,
            ReaderCount = dto.ReaderCount,
            CollectedCount = dto.CollectedCount,
            CommentCount = dto.CommentCount,
            CanComment = dto.CanComment,
            Entries = dto.Entries ?? [],
            Peripheries = dto.Peripheries ?? [],
            PeripheryOverviewModels = dto.PeripheryOverviewModels ?? [],
            Pictures = dto.Pictures ?? [],
        };
    }
}
