using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Stores;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class StoreInfoQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<StoreInfoQueryService> logger) : QueryServiceBase(httpClient), IStoreInfoQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan StoreInfoCacheDuration = TimeSpan.FromMinutes(5);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<IReadOnlyList<DiscountGameItem>>> GetAllGameStoreInfoAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:all-game-store-info";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<DiscountGameItem>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<DiscountGameItem>>(cached, cacheRead);
        }

        var result = await GetAsync<List<StoreInfoCardModel>>(
            "api/storeinfo/GetAllGameStoreInfo",
            "STORE_INFO",
            "商店折扣信息",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<DiscountGameItem>>.Fail(
                result.Error?.Code ?? "STORE_INFO_FAILED",
                result.Error?.Message ?? "获取商店折扣信息失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<DiscountGameItem> items = result.Data.Select(MapToViewModel).ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, items, StoreInfoCacheDuration, cacheRead);

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<IReadOnlyList<DiscountGameItem>>(items, cacheRead);
    }

    private static DiscountGameItem MapToViewModel(StoreInfoCardModel dto)
    {
        return new DiscountGameItem
        {
            Id = dto.Id,
            Name = dto.Name ?? "",
            BriefIntroduction = dto.BriefIntroduction ?? "",
            MainImage = dto.MainImage ?? "",
            PublishTime = dto.PublishTime,
            OriginalPrice = dto.OriginalPrice ?? 0,
            PriceNow = dto.PriceNow ?? 0,
            CutNow = dto.CutNow ?? 0,
            PriceLowest = dto.PriceLowest ?? 0,
            CutLowest = dto.CutLowest ?? 0,
            EvaluationCount = dto.EvaluationCount ?? 0,
            RecommendationRate = dto.RecommendationRate ?? 0,
            PlayTime = dto.PlayTime ?? 0,
        };
    }

    public async Task<SdkResult<GameRevenueInfoViewModel>> GetGameRevenueInfoAsync(
        int year, int page, int max, int order, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:game-revenue-info:{year}:{page}:{max}:{order}";
        if (memoryCache.TryGetValue(cacheKey, out GameRevenueInfoViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<GameRevenueInfoViewModel>(cached, cacheRead);
        }

        var result = await GetAsync<GameRevenueInfoViewModel>(
            $"api/storeinfo/GetGameRevenueInfo?year={year}&page={page}&max={max}&order={order}",
            "STORE_REVENUE_INFO",
            "游戏销量信息",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<GameRevenueInfoViewModel>.Fail(
                result.Error?.Code ?? "STORE_REVENUE_INFO_FAILED",
                result.Error?.Message ?? "获取游戏销量信息失败",
                result.Error?.StatusCode);
        }

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, result.Data, StoreInfoCacheDuration, cacheRead);

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<GameRevenueInfoViewModel>(result.Data, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<CnGalGenerationYearItem>>> GetCnGalGenerationAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:cngal-generation";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<CnGalGenerationYearItem>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<CnGalGenerationYearItem>>(cached, cacheRead);
        }

        var result = await GetAsync<List<CnGalGenerationYearModel>>(
            "api/storeinfo/GetCnGalGeneration",
            "CNGAL_GENERATION",
            "CnGal 世代列表",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<CnGalGenerationYearItem>>.Fail(
                result.Error?.Code ?? "CNGAL_GENERATION_FAILED",
                result.Error?.Message ?? "获取 CnGal 世代数据失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<CnGalGenerationYearItem> items = [.. result.Data
            .OrderBy(year => year.Year)
            .Select(year => new CnGalGenerationYearItem
            {
                Year = year.Year,
                Games = [.. (year.Games ?? []).Select(game => new CnGalGenerationGameItem
                {
                    Name = game?.Name ?? string.Empty,
                })],
            })];

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, items, StoreInfoCacheDuration, cacheRead);

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<IReadOnlyList<CnGalGenerationYearItem>>(items, cacheRead);
    }
}
