using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;

namespace CnGalWebSite.SDK.MainSite.Queries;

/// <summary>Small public-endpoint queries without home-page fan-out. Authentication is determined by the supplied HttpClient.</summary>
public sealed class RobotQueryService(HttpClient httpClient) : IRobotQueryService
{
    private const string Site = "https://www.cngal.org";

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken, bool allowNotFound = false)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent ||
            (allowNotFound && response.StatusCode == HttpStatusCode.NotFound))
        {
            return default;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(SdkJsonSerializerOptions.Default, cancellationToken);
    }

    private static string Text(string? value) => WebUtility.HtmlDecode(
        Regex.Replace(value ?? "", "<[^>]*>", " ", RegexOptions.None, TimeSpan.FromSeconds(1))).Trim();

    private static string Url(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Site + "/";
        return Uri.TryCreate(new Uri(Site), value, out var uri) && uri.Scheme is "http" or "https"
            ? uri.AbsoluteUri : Site + "/";
    }

    private static RobotQueryResult Result(IEnumerable<RobotItem> items, string source, int? total = null, int page = 1)
        => new() { Items = items.Take(6).ToList(), SourceUrl = Url(source), Total = total, Page = page };

    public async Task<RobotQueryResult> SearchAsync(string text, int currentPage = 1, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (text.Length > 200 || currentPage < 1 || currentPage > 1000) throw new ArgumentOutOfRangeException(nameof(currentPage));
        var data = await GetAsync<SearchViewModel>($"api/home/search?Text={Uri.EscapeDataString(text)}&Page={currentPage}&Types=Entry&Types=Article", cancellationToken);
        var items = data?.pagedResultDto?.Data?.SelectMany(x =>
            x.entry is not null ? new[] { new RobotItem { Id = x.entry.Id, Title = Text(x.entry.Name), Summary = Text(x.entry.BriefIntroduction), Url = $"{Site}/entries/{x.entry.Id}" } } :
            x.article is not null ? new[] { new RobotItem { Id = x.article.Id, Kind = "article", Title = Text(string.IsNullOrEmpty(x.article.DisplayName) ? x.article.Name : x.article.DisplayName), Summary = Text(x.article.BriefIntroduction), Url = $"{Site}/articles/{x.article.Id}" } } : Array.Empty<RobotItem>()) ?? [];
        return Result(items, $"/search?Text={Uri.EscapeDataString(text)}&Page={currentPage}", data?.pagedResultDto?.TotalCount, currentPage);
    }

    public async Task<RobotQueryResult> GetEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        var entry = await GetAsync<EntryIndexViewModel>($"api/entries/GetEntryView/{id}", cancellationToken, allowNotFound: true);
        var source = $"{Site}/entries/{id}";
        return Result(entry is null || entry.IsHidden ? [] : new[] { new RobotItem { Id = id, Title = Text(entry.Name), Summary = Text(entry.BriefIntroduction), Url = source } }, source);
    }

    public async Task<RobotQueryResult> GetArticleAsync(long id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        var article = await GetAsync<ArticleViewModel>($"api/articles/GetArticleView/{id}", cancellationToken, allowNotFound: true);
        var source = $"{Site}/articles/{id}";
        return Result(article is null || article.IsHidden ? [] : new[] { new RobotItem { Id = id, Kind = "article", Title = Text(string.IsNullOrEmpty(article.DisplayName) ? article.Name : article.DisplayName), Summary = Text(article.BriefIntroduction), Url = source } }, source);
    }

    private async Task<RobotQueryResult> HomeAsync<T>(string action, Func<T, string> summary, CancellationToken cancellationToken) where T : HomeItemModel
    {
        var items = await GetAsync<List<T>>($"api/home/{action}", cancellationToken) ?? [];
        return Result(items.Select(x => new RobotItem { Title = Text(x.Name), Summary = Text(summary(x)), Url = Url(x.Url) }), Site);
    }
    public Task<RobotQueryResult> GetLatestGamesAsync(CancellationToken cancellationToken = default) => HomeAsync<PublishedGameItemModel>("ListPublishedGames", x => x.BriefIntroduction, cancellationToken);
    public Task<RobotQueryResult> GetLatestDemoGamesAsync(CancellationToken cancellationToken = default) => HomeAsync<RecentlyDemoGameItemModel>("ListRecentlyDemoGames", x => x.BriefIntroduction, cancellationToken);
    public Task<RobotQueryResult> GetUpcomingGamesAsync(CancellationToken cancellationToken = default) => HomeAsync<UpcomingGameItemModel>("ListUpcomingGames", x => $"{x.PublishTime} {x.BriefIntroduction}", cancellationToken);
    public Task<RobotQueryResult> GetRecentlyUpdatedGamesAsync(CancellationToken cancellationToken = default) => HomeAsync<RecentlyEditedGameItemModel>("ListRecentlyEditedGames", x => x.PublishTime, cancellationToken);

    public async Task<RobotQueryResult> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        var items = await GetAsync<List<HomeNewsAloneViewModel>>("api/home/GetHomeNewsView", cancellationToken) ?? [];
        return Result(items.Select(x => new RobotItem { Id = x.ArticleId, Kind = "article", Title = Text(x.Title), Summary = Text(x.Text), Url = Url(x.Link) }), Site);
    }

    public async Task<RobotQueryResult> GetRandomGameAsync(CancellationToken cancellationToken = default)
    {
        var item = await GetAsync<RobotItem>("api/robot/v2/random-game", cancellationToken);
        return Result(item is null ? [] : new[] { new RobotItem { Id = item.Id, Title = Text(item.Title),
            Summary = Text(item.Summary), Url = Url(string.IsNullOrWhiteSpace(item.Url) ? $"/entries/{item.Id}" : item.Url) } }, item?.Url ?? Site);
    }

    public async Task<RobotQueryResult> GetRoleBirthdaysAsync(int? month = null, int? day = null, CancellationToken cancellationToken = default)
    {
        if (month.HasValue != day.HasValue) throw new ArgumentException("month and day must be supplied together.");
        var today = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Asia/Shanghai");
        var date = new DateOnly(2000, month ?? today.Month, day ?? today.Day);
        var items = await GetAsync<List<RobotItem>>($"api/robot/v2/role-birthdays?month={date.Month}&day={date.Day}", cancellationToken) ?? [];
        return Result(items.Select(x => new RobotItem { Id = x.Id, Title = Text(x.Title), Summary = Text(x.Summary),
            Url = Url(string.IsNullOrWhiteSpace(x.Url) ? $"/entries/{x.Id}" : x.Url) }), Site);
    }

    public async Task<RobotDiscountSummary> GetDiscountSummaryAsync(CancellationToken cancellationToken = default)
        => await GetAsync<RobotDiscountSummary>("api/robot/v2/discount-summary", cancellationToken) ?? throw new HttpRequestException("Discount summary unavailable.");
}
