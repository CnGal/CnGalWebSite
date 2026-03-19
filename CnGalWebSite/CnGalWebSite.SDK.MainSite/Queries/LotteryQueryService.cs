using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class LotteryQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<LotteryQueryService> logger) : QueryServiceBase(httpClient), ILotteryQueryService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string DetailPathTemplate = "api/lotteries/GetLotteryView/{0}";
    private const string CardsPath = "api/lotteries/GetLotteryCards";
    private const string CardsCacheKey = "main-site:lottery-cards";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<LotteryDetailViewModel>> GetLotteryDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:lottery-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out LotteryDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<LotteryDetailViewModel>.Ok(cached);
        }

        var path = string.Format(DetailPathTemplate, id);
        var result = await GetSingleAsync<LotteryViewModel, LotteryDetailViewModel>(
            path,
            MapToViewModel,
            "LOTTERY",
            "抽奖",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, CacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<IReadOnlyList<LotteryCardItemModel>>> GetLotteryCardsAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(CardsCacheKey, out IReadOnlyList<LotteryCardItemModel>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<LotteryCardItemModel>>.Ok(cached);
        }

        var result = await GetAsync<List<LotteryCardViewModel>>(CardsPath, "LOTTERY_CARDS", "抽奖列表", cancellationToken);
        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<LotteryCardItemModel>>.Fail(
                result.Error?.Code ?? "LOTTERY_CARDS_FAILED",
                result.Error?.Message ?? "获取抽奖列表失败");
        }

        var items = result.Data
            .Select(MapToCardItem)
            .ToList() as IReadOnlyList<LotteryCardItemModel>;

        memoryCache.Set(CardsCacheKey, items, CacheDuration);
        return SdkResult<IReadOnlyList<LotteryCardItemModel>>.Ok(items);
    }

    private static LotteryCardItemModel MapToCardItem(LotteryCardViewModel dto)
    {
        var now = DateTime.Now;
        return new LotteryCardItemModel
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            BriefIntroduction = dto.BriefIntroduction ?? string.Empty,
            MainPicture = dto.MainPicture ?? string.Empty,
            Thumbnail = dto.Thumbnail ?? string.Empty,
            BeginTime = dto.BeginTime,
            EndTime = dto.EndTime,
            Count = dto.Count,
            IsEnd = dto.EndTime < now,
        };
    }

    private static LotteryDetailViewModel MapToViewModel(LotteryViewModel lottery)
    {
        return new LotteryDetailViewModel
        {
            Id = lottery.Id,
            Name = lottery.Name ?? string.Empty,
            DisplayName = lottery.DisplayName ?? string.Empty,
            BriefIntroduction = lottery.BriefIntroduction ?? string.Empty,
            MainPicture = lottery.MainPicture ?? string.Empty,
            BackgroundPicture = lottery.BackgroundPicture ?? string.Empty,
            SmallBackgroundPicture = lottery.SmallBackgroundPicture ?? string.Empty,
            MainPage = lottery.MainPage ?? string.Empty,
            Type = lottery.Type,
            ConditionType = lottery.ConditionType,
            BeginTime = lottery.BeginTime,
            EndTime = lottery.EndTime,
            LotteryTime = lottery.LotteryTime,
            CreateTime = lottery.CreateTime,
            LastEditTime = lottery.LastEditTime,
            IsEnd = lottery.IsEnd,
            IsHidden = lottery.IsHidden,
            CanComment = lottery.CanComment,
            Count = lottery.Count,
            ReaderCount = lottery.ReaderCount,
            CommentCount = lottery.CommentCount,
            Awards = lottery.Awards?
                .OrderByDescending(a => a.Priority)
                .Select(a => new LotteryAwardDetailModel
                {
                    Id = a.Id,
                    Name = a.Name ?? string.Empty,
                    Priority = a.Priority,
                    Count = a.Count,
                    Type = a.Type,
                    Sponsor = a.Sponsor ?? string.Empty,
                    Image = a.Image ?? string.Empty,
                    Link = a.Link ?? string.Empty,
                    Integral = a.Integral,
                    Winners = a.Users?
                        .Select(u => new LotteryWinnerModel
                        {
                            Id = u.Id ?? string.Empty,
                            Name = u.Name ?? string.Empty,
                            PhotoPath = u.PhotoPath ?? string.Empty,
                        })
                        .ToList() ?? [],
                })
                .ToList() ?? [],
        };
    }
}
