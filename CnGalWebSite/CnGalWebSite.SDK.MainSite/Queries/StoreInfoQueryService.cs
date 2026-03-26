using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class StoreInfoQueryService(
    HttpClient httpClient,
    ILogger<StoreInfoQueryService> logger) : QueryServiceBase(httpClient), IStoreInfoQueryService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<IReadOnlyList<DiscountGameItem>>> GetAllGameStoreInfoAsync(CancellationToken cancellationToken = default)
    {
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
        return SdkResult<IReadOnlyList<DiscountGameItem>>.Ok(items);
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
}
