using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class EntryQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<EntryQueryService> logger) : QueryServiceBase(httpClient), IEntryQueryService
{
    private static readonly TimeSpan EntryCacheDuration = TimeSpan.FromMinutes(5);
    private const string EntryDetailPathTemplate = "api/entries/GetEntryView/{0}";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<EntryDetailViewModel>> GetEntryDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:entry-detail:{id}";
        if (memoryCache.TryGetValue(cacheKey, out EntryDetailViewModel? cached) && cached is not null)
        {
            return SdkResult<EntryDetailViewModel>.Ok(cached);
        }

        var path = string.Format(EntryDetailPathTemplate, id);
        var result = await GetSingleAsync<EntryIndexViewModel, EntryDetailViewModel>(
            path,
            MapToViewModel,
            "ENTRY",
            "条目",
            id,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, EntryCacheDuration);
        }

        return result;
    }

    private static EntryDetailViewModel MapToViewModel(EntryIndexViewModel entry)
    {
        return new EntryDetailViewModel
        {
            Id = entry.Id,
            Name = entry.Name ?? string.Empty,
            AnotherName = entry.AnotherName ?? string.Empty,
            BriefIntroduction = entry.BriefIntroduction ?? string.Empty,
            Type = entry.Type,
            MainPicture = entry.MainPicture ?? string.Empty,
            Thumbnail = entry.Thumbnail ?? string.Empty,
            BackgroundPicture = entry.BackgroundPicture ?? string.Empty,
            SmallBackgroundPicture = entry.SmallBackgroundPicture ?? string.Empty,
            IsEdit = entry.IsEdit,
            IsHidden = entry.IsHidden,
            IsHideOutlink = entry.IsHideOutlink,
            IsScored = entry.IsScored,
            CanComment = entry.CanComment,
            TabIndex = entry.TabIndex <= 0 ? 1 : entry.TabIndex,
            MainPage = entry.MainPage ?? string.Empty,
            Booking = entry.Booking,
            Information = entry.Information?.ToList() ?? [],
            Tags = entry.Tags?
                .Where(s => string.IsNullOrWhiteSpace(s.Name) is false)
                .DistinctBy(s => s.Id)
                .Take(24)
                .ToList() ?? [],
            Audio = entry.Audio?
                .Where(s => string.IsNullOrWhiteSpace(s.Url) is false)
                .OrderBy(s => s.Priority)
                .ToList() ?? [],
            Pictures = entry.Pictures?
                .SelectMany(s => s.Pictures ?? [])
                .Where(s => string.IsNullOrWhiteSpace(s.Url) is false)
                .OrderBy(s => s.Priority)
                .ToList() ?? [],
            NewsOfEntry = entry.NewsOfEntry?
                .OrderByDescending(s => s.HappenedTime)
                .ToList() ?? [],
            EntryRelevances = entry.EntryRelevances?.ToList() ?? [],
            ArticleRelevances = entry.ArticleRelevances?.ToList() ?? [],
            VideoRelevances = entry.VideoRelevances?.ToList() ?? [],
            OtherRelevances = entry.OtherRelevances?.ToList() ?? [],
            Roles = entry.Roles?.ToList() ?? [],
            StaffGames = entry.StaffGames?.ToList() ?? [],
            Staffs = entry.Staffs?.ToList() ?? [],
            ProductionGroups = entry.ProductionGroups?.ToList() ?? [],
            Publishers = entry.Publishers?.ToList() ?? [],
            Releases = entry.Releases?
                .OrderBy(s => s.Time ?? DateTime.MaxValue)
                .ToList() ?? []
        };
    }
}
