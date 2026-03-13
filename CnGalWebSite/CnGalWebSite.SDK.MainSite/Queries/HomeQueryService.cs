using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class HomeQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<HomeQueryService> logger) : QueryServiceBase(httpClient), IHomeQueryService
{
    private static readonly TimeSpan HomeCacheDuration = TimeSpan.FromMinutes(2);

    public async Task<SdkResult<HomeSummaryViewModel>> GetHomeSummaryAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:home-summary";
        if (memoryCache.TryGetValue(cacheKey, out HomeSummaryViewModel? cached) && cached is not null)
        {
            return SdkResult<HomeSummaryViewModel>.Ok(cached);
        }

        var warnings = new List<SdkErrorModel>();

        var carouselsResult = await GetListAsync<HomeCarouselApiItem>(
            "api/home/GetHomeCarouselsView",
            "HOME_CAROUSELS_FAILED",
            warnings,
            cancellationToken);

        var publishedGamesResult = await GetListAsync<PublishedGameItemModel>(
            "api/home/ListPublishedGames",
            "HOME_PUBLISHED_GAMES_FAILED",
            warnings,
            cancellationToken);

        var announcementsResult = await GetListAsync<AnnouncementItemModel>(
            "api/home/ListAnnouncements",
            "HOME_ANNOUNCEMENTS_FAILED",
            warnings,
            cancellationToken);

        var latestArticlesResult = await GetListAsync<LatestArticleItemModel>(
            "api/home/ListLatestArticles",
            "HOME_LATEST_ARTICLES_FAILED",
            warnings,
            cancellationToken);

        if (warnings.Count == 4)
        {
            return SdkResult<HomeSummaryViewModel>.Fail("HOME_ALL_REQUESTS_FAILED", "首页数据加载失败，请稍后重试");
        }

        var model = new HomeSummaryViewModel
        {
            HeroTitle = "CnGal 资料站",
            HeroSubtitle = "愿每一个 CnGal 创作者的作品都能不被忘记",
            Carousels = carouselsResult.Select(s => new HomeCarouselViewModel
            {
                Image = s.Image ?? string.Empty,
                Link = s.Link ?? string.Empty,
                Note = s.Note ?? string.Empty
            }).ToList(),
            FeaturedEntries = publishedGamesResult.Take(8).Select(MapFeaturedEntry).ToList(),
            Announcements = announcementsResult.Take(8).Select(s => new HomeAnnouncementViewModel
            {
                Name = s.Name ?? string.Empty,
                Url = NormalizePath(s.Url),
                BriefIntroduction = s.BriefIntroduction ?? string.Empty
            }).ToList(),
            LatestArticles = latestArticlesResult.Take(8).Select(s => new HomeArticleViewModel
            {
                Name = s.Name ?? string.Empty,
                Url = NormalizePath(s.Url),
                Author = string.IsNullOrWhiteSpace(s.OriginalAuthor) ? (s.UserName ?? "未知") : s.OriginalAuthor,
                PublishTime = s.PublishTime ?? string.Empty
            }).ToList(),
            Warnings = warnings
        };

        memoryCache.Set(cacheKey, model, HomeCacheDuration);
        return SdkResult<HomeSummaryViewModel>.Ok(model);
    }

    private async Task<IReadOnlyList<TItem>> GetListAsync<TItem>(
        string path,
        string errorCode,
        ICollection<SdkErrorModel> warnings,
        CancellationToken cancellationToken)
    {
        try
        {
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "首页接口请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}; ResponseBody={ResponseBody}",
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    errorCode,
                    TrimForLog(responseBody));

                warnings.Add(new SdkErrorModel
                {
                    Code = errorCode,
                    Message = $"请求 {path} 失败（HTTP {(int)response.StatusCode}）",
                    StatusCode = (int)response.StatusCode
                });
                return [];
            }

            try
            {
                var data = Deserialize<List<TItem>>(responseBody);
                return data ?? [];
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "首页接口反序列化失败。Path={Path}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}; ResponseBody={ResponseBody}",
                    path,
                    HttpClient.BaseAddress,
                    errorCode,
                    TrimForLog(responseBody));
                warnings.Add(new SdkErrorModel
                {
                    Code = errorCode,
                    Message = $"请求 {path} 成功但数据格式不兼容",
                    StatusCode = 200
                });
                return [];
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "首页接口请求异常。Path={Path}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}",
                path,
                HttpClient.BaseAddress,
                errorCode);
            warnings.Add(new SdkErrorModel
            {
                Code = errorCode,
                Message = $"请求 {path} 时发生异常",
                StatusCode = null
            });
            return [];
        }
    }

    private static HomeFeaturedEntryViewModel MapFeaturedEntry(PublishedGameItemModel item)
    {
        var id = ExtractEntryId(item.Url);
        var tagline = item.Tags is { Count: > 0 }
            ? string.Join(" · ", item.Tags.Take(3))
            : item.BriefIntroduction ?? string.Empty;

        return new HomeFeaturedEntryViewModel
        {
            Id = id,
            Name = item.Name ?? string.Empty,
            Tagline = tagline
        };
    }

    private static int ExtractEntryId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return 0;
        }

        var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var last = segments.LastOrDefault();
        return int.TryParse(last, out var id) ? id : 0;
    }

    private static string NormalizePath(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "#";
        }

        return url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : $"/{url.TrimStart('/')}";
    }

    private sealed class HomeCarouselApiItem
    {
        public string? Image { get; set; }

        public string? Link { get; set; }

        public string? Note { get; set; }
    }
}
