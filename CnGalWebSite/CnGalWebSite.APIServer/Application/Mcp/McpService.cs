#nullable enable
using System.Net;
using System.Text.RegularExpressions;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Home;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Stores;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Mcp;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.APIServer.Application.Mcp;

/// <summary>直接调用 API 内部服务，整理为 MCP 工具使用的简要结果。</summary>
public sealed class McpService(
    IHomeService home, ISearchHelper search, IEntryService entries, IArticleService articles,
    IStoreInfoService stores)
{
    private const int MaxItems = 6;

    private static string Text(string? value) => WebUtility.HtmlDecode(
        Regex.Replace(value ?? "", "<[^>]*>", " ", RegexOptions.None, TimeSpan.FromSeconds(1))).Trim();

    private static McpQueryResult Result(IEnumerable<McpItem> items, string source = "/", int? total = null, int page = 1)
        => new() { Items = items.Take(MaxItems).ToList(), SourceUrl = source, Total = total, Page = page };

    private static McpItem Item(EntryInforTipViewModel entry) => new()
    {
        Id = entry.Id, Title = Text(entry.Name), Summary = Text(entry.BriefIntroduction),
        Url = $"entries/{entry.Id}"
    };

    private static McpItem Item(ArticleInforTipViewModel article) => new()
    {
        Id = article.Id, Kind = "article", Title = Text(string.IsNullOrWhiteSpace(article.DisplayName) ? article.Name : article.DisplayName),
        Summary = Text(article.BriefIntroduction), Url = $"articles/{article.Id}"
    };

    public async Task<McpQueryResult> SearchAsync(string text, int currentPage = 1, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (text.Length > 200) throw new ArgumentOutOfRangeException(nameof(text));
        if (currentPage < 1 || currentPage > 1000) throw new ArgumentOutOfRangeException(nameof(currentPage));
        var input = SearchInputModel.Parse(["Entry", "Article"], [], text, SearchSortField.Default, SortDirection.Descending, 1);
        // 网页与 MCP 每页数量不同，来源链接保留搜索条件并指向第一页。
        var sourceUrl = $"search{input.ToPageQueryString()}";
        input.CurrentPage = currentPage;
        input.MaxResultCount = MaxItems;
        var data = await search.QueryAsync(input).WaitAsync(cancellationToken);
        var items = data.Data.SelectMany(x => x.entry is not null ? new[] { Item(x.entry) } :
            x.article is not null ? new[] { Item(x.article) } : Array.Empty<McpItem>());
        return Result(items, sourceUrl, data.TotalCount, currentPage);
    }

    public async Task<McpQueryResult> GetEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        var entry = await entries.GetPublicSummaryAsync(id, cancellationToken);
        return Result(entry is null ? [] : [Item(entry)], $"entries/{id}");
    }

    public async Task<McpQueryResult> GetArticleAsync(long id, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        var article = await articles.GetPublicSummaryAsync(id, cancellationToken);
        return Result(article is null ? [] : [Item(article)], $"articles/{id}");
    }

    private static async Task<McpQueryResult> HomeAsync<T>(Func<Task<List<T>>> query, Func<T, string?> summary, CancellationToken cancellationToken) where T : HomeItemModel
    {
        var items = await query().WaitAsync(cancellationToken);
        return Result(items.Select(x => new McpItem { Id = x.Id, Title = Text(x.Name), Summary = Text(summary(x)), Url = x.Url }));
    }

    public Task<McpQueryResult> GetLatestGamesAsync(CancellationToken cancellationToken = default)
        => HomeAsync(home.ListPublishedGames, x => x.BriefIntroduction, cancellationToken);
    public Task<McpQueryResult> GetLatestDemoGamesAsync(CancellationToken cancellationToken = default)
        => HomeAsync(home.ListRecentlyDemoGames, x => x.BriefIntroduction, cancellationToken);
    public Task<McpQueryResult> GetUpcomingGamesAsync(CancellationToken cancellationToken = default)
        => HomeAsync(home.ListUpcomingGames, x => $"{x.PublishTime} {x.BriefIntroduction}", cancellationToken);
    public Task<McpQueryResult> GetRecentlyUpdatedGamesAsync(CancellationToken cancellationToken = default)
        => HomeAsync(home.ListRecentlyEditedGames, x => x.PublishTime, cancellationToken);

    public async Task<McpQueryResult> GetNewsAsync(CancellationToken cancellationToken = default)
    {
        var items = await home.GetHomeNewsViewAsync().WaitAsync(cancellationToken);
        return Result(items.Select(x => new McpItem
        {
            Id = x.ArticleId, Kind = "article", Title = Text(x.Title), Summary = Text(x.Text), Url = x.Link
        }));
    }

    public async Task<McpQueryResult> GetRandomGameAsync(CancellationToken cancellationToken = default)
    {
        var id = await entries.GetRandomPublicGameIdAsync(cancellationToken);
        return id.HasValue ? await GetEntryAsync(id.Value, cancellationToken) : Result([]);
    }

    public async Task<McpQueryResult> GetRoleBirthdaysAsync(int? month = null, int? day = null, CancellationToken cancellationToken = default)
    {
        if (month.HasValue != day.HasValue) throw new ArgumentException("月份和日期必须同时填写，或同时留空。");
        var today = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.UtcNow, "Asia/Shanghai");
        var selectedDay = day ?? today.Day;
        // day 为 0 时查询整月；使用闰年校验日期，以支持 2 月 29 日。
        var date = new DateOnly(2000, month ?? today.Month, selectedDay == 0 ? 1 : selectedDay);
        var roles = await entries.GetBirthdayRoles(date.Month, selectedDay).WaitAsync(cancellationToken);
        return Result(roles.DistinctBy(x => x.Id).OrderBy(x => x.Id).Select(Item));
    }

    public async Task<McpDiscountSummary> GetDiscountSummaryAsync(CancellationToken cancellationToken = default)
        => new() { Count = await stores.CountDiscountedGamesAsync(cancellationToken), Url = "discount" };
}
