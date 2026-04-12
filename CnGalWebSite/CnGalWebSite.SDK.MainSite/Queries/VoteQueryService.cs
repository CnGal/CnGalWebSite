using CnGalWebSite.DataModel.ViewModel.Votes;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class VoteQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<VoteQueryService> logger) : QueryServiceBase(httpClient), IVoteQueryService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private const string CardsPath = "api/votes/GetVoteCards";
    private const string CardsCacheKey = "main-site:vote-cards";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<IReadOnlyList<VoteCardItemModel>>> GetVoteCardsAsync(CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(CardsCacheKey, out IReadOnlyList<VoteCardItemModel>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<VoteCardItemModel>>.Ok(cached);
        }

        var result = await GetAsync<List<VoteCardViewModel>>(CardsPath, "VOTE_CARDS", "投票列表", cancellationToken);
        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<VoteCardItemModel>>.Fail(
                result.Error?.Code ?? "VOTE_CARDS_FAILED",
                result.Error?.Message ?? "获取投票列表失败");
        }

        var items = result.Data
            .Select(MapToCardItem)
            .ToList() as IReadOnlyList<VoteCardItemModel>;

        memoryCache.Set(CardsCacheKey, items, CacheDuration);
        return SdkResult<IReadOnlyList<VoteCardItemModel>>.Ok(items);
    }

    private static VoteCardItemModel MapToCardItem(VoteCardViewModel dto)
    {
        var now = DateTime.Now;
        return new VoteCardItemModel
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            BriefIntroduction = dto.BriefIntroduction ?? string.Empty,
            MainPicture = dto.MainPicture ?? string.Empty,
            BeginTime = dto.BeginTime,
            EndTime = dto.EndTime,
            Count = dto.Count,
            IsEnd = dto.EndTime < now,
        };
    }
}
