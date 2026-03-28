using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class PeripheryQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<PeripheryQueryService> logger) : QueryServiceBase(httpClient), IPeripheryQueryService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    protected override ILogger Logger => logger;

    public Task<SdkResult<PeripheryDetailViewModel>> GetPeripheryDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        return GetSingleAsync<PeripheryViewModel, PeripheryDetailViewModel>(
            $"api/peripheries/GetPeripheryView/{id}",
            MapToViewModel,
            "PERIPHERY",
            "周边",
            id,
            cancellationToken);
    }

    public async Task<SdkResult<GameOverviewPeripheryListModel>> GetEntryOverviewPeripheriesAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:entry-periphery-overview:{entryId}";
        if (memoryCache.TryGetValue(cacheKey, out GameOverviewPeripheryListModel? cached) && cached is not null)
        {
            return SdkResult<GameOverviewPeripheryListModel>.Ok(cached);
        }

        var path = $"api/peripheries/GetEntryOverviewPeripheries/{entryId}";
        var result = await GetAsync<GameOverviewPeripheryListModel>(
            path,
            "PERIPHERY",
            "词条周边概览",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, CacheDuration);
        }

        return result;
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
