using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class EntryQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<EntryQueryService> logger) : QueryServiceBase(httpClient), IEntryQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan EntryCacheDuration = TimeSpan.FromMinutes(5);
    private const string EntryDetailPathTemplate = "api/entries/GetEntryView/{0}";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<EntryDetailViewModel>> GetEntryDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = PublicCacheKeys.Detail("entry", id);
        if (memoryCache.TryGetValue(cacheKey, out EntryDetailViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<EntryDetailViewModel>(cached, cacheRead);
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
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, EntryCacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<GamePublishTimesCardItem>>> GetPublishGamesByTimeAsync(
        int year, int month, int mode = 0, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:publish-games-by-time:{year}:{month}:{mode}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<GamePublishTimesCardItem>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<GamePublishTimesCardItem>>(cached, cacheRead);
        }

        var path = $"api/entries/GetPublishGamesByTime?year={year}&month={month}&mode={mode}";

        var result = await GetAsync<List<EntryInforTipViewModel>>(
            path, "ENTRY_PUBLISH_TIMES", "游戏发布时间列表", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<GamePublishTimesCardItem>>.Fail(
                result.Error?.Code ?? "ENTRY_PUBLISH_TIMES_FAILED",
                result.Error?.Message ?? "获取游戏发布时间列表失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<GamePublishTimesCardItem> items = result.Data
            .Select(MapToCardItem)
            .OrderBy(x => x.PublishTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, items, EntryCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<IReadOnlyList<GamePublishTimesCardItem>>(items, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<GamePublishTimesTimelineItem>>> GetPublishGamesTimelineAsync(
        long afterTime, long beforeTime, int mode = 0, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:publish-games-timeline:{afterTime}:{beforeTime}:{mode}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<GamePublishTimesTimelineItem>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<GamePublishTimesTimelineItem>>(cached, cacheRead);
        }

        var path = $"api/entries/GetPublishGamesTimeline?afterTime={afterTime}&beforeTime={beforeTime}&mode={mode}";

        var result = await GetAsync<List<PublishGamesTimelineModel>>(
            path, "ENTRY_PUBLISH_TIMES", "游戏发布时间线", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<GamePublishTimesTimelineItem>>.Fail(
                result.Error?.Code ?? "ENTRY_PUBLISH_TIMELINE_FAILED",
                result.Error?.Message ?? "获取游戏发布时间线失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<GamePublishTimesTimelineItem> items = result.Data
            .Select(MapToTimelineItem)
            .OrderByDescending(x => x.PublishTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, items, EntryCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<IReadOnlyList<GamePublishTimesTimelineItem>>(items, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<BirthdayCalendarItem>>> GetRoleBirthdaysByMonthAsync(
        int month, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:role-birthdays-month:{month}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<BirthdayCalendarItem>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<BirthdayCalendarItem>>(cached, cacheRead);
        }

        var path = $"api/entries/GetRoleBirthdaysByTime?month={month}";
        var result = await GetAsync<List<RoleBrithdayViewModel>>(
            path, "ENTRY_BIRTHDAYS", "角色生日列表", cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<BirthdayCalendarItem>>.Fail(
                result.Error?.Code ?? "ENTRY_BIRTHDAYS_FAILED",
                result.Error?.Message ?? "获取角色生日列表失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<BirthdayCalendarItem> items = result.Data
            .Select(MapToBirthdayItem)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, items, EntryCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.Complete<IReadOnlyList<BirthdayCalendarItem>>(items, cacheRead);
    }

    private static GamePublishTimesCardItem MapToCardItem(EntryInforTipViewModel dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? "",
        Image = dto.MainImage ?? "",
        BriefIntroduction = dto.BriefIntroduction ?? "",
        PublishTime = dto.PublishTime
    };

    private static GamePublishTimesTimelineItem MapToTimelineItem(PublishGamesTimelineModel dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? "",
        Image = dto.MainImage ?? "",
        BriefIntroduction = dto.BriefIntroduction ?? "",
        Thumbnail = dto.Thumbnail ?? "",
        PublishTime = dto.PublishTime,
        PublishTimeNote = dto.PublishTimeNote
    };

    private static BirthdayCalendarItem MapToBirthdayItem(RoleBrithdayViewModel dto)
    {
        var gameInfor = dto.AddInfors?.FirstOrDefault(x => x.Modifier == "登场游戏");
        var gameName = gameInfor?.Contents?.FirstOrDefault()?.DisplayName;

        return new BirthdayCalendarItem
        {
            Id = dto.Id,
            Name = dto.Name ?? "",
            Image = dto.MainImage ?? "",
            BriefIntroduction = dto.BriefIntroduction ?? "",
            BirthdayMonth = dto.Brithday.Month,
            BirthdayDay = dto.Brithday.Day,
            GameName = gameName
        };
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

