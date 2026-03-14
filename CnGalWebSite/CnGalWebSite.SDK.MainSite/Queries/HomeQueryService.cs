using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
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
        const int totalRequests = 19;

        var carouselsResult = await GetListAsync<CarouselViewModel>("api/home/GetHomeCarouselsView", "HOME_CAROUSELS_FAILED", warnings, cancellationToken);
        var hotRecommendsResult = await GetListAsync<HotRecommendItemModel>("api/home/ListHotRecommends", "HOME_HOT_RECOMMENDS_FAILED", warnings, cancellationToken);
        var publishedGamesResult = await GetListAsync<PublishedGameItemModel>("api/home/ListPublishedGames", "HOME_PUBLISHED_GAMES_FAILED", warnings, cancellationToken);
        var recentlyDemoGamesResult = await GetListAsync<RecentlyDemoGameItemModel>("api/home/ListRecentlyDemoGames", "HOME_RECENTLY_DEMO_GAMES_FAILED", warnings, cancellationToken);
        var upcomingGamesResult = await GetListAsync<UpcomingGameItemModel>("api/home/ListUpcomingGames", "HOME_UPCOMING_GAMES_FAILED", warnings, cancellationToken);
        var activityCarouselsResult = await GetListAsync<CarouselViewModel>("api/home/GetActivityCarouselsView", "HOME_ACTIVITY_CAROUSELS_FAILED", warnings, cancellationToken);
        var homeNewsResult = await GetListAsync<HomeNewsAloneViewModel>("api/home/GetHomeNewsView", "HOME_NEWS_FAILED", warnings, cancellationToken);
        var weeklyNewsResult = await GetListAsync<ArticleInforTipViewModel>("api/news/GetWeeklyNewsOverview", "HOME_WEEKLY_NEWS_FAILED", warnings, cancellationToken);
        var latestArticlesResult = await GetListAsync<LatestArticleItemModel>("api/home/ListLatestArticles", "HOME_LATEST_ARTICLES_FAILED", warnings, cancellationToken);
        var latestVideosResult = await GetListAsync<LatestVideoItemModel>("api/home/ListLatestVideos", "HOME_LATEST_VIDEOS_FAILED", warnings, cancellationToken);
        var friendLinksResult = await GetListAsync<FriendLinkItemModel>("api/home/ListFriendLinks", "HOME_FRIEND_LINKS_FAILED", warnings, cancellationToken);
        var birthdaysResult = await GetListAsync<RoleBrithdayViewModel>(
            $"api/entries/GetRoleBirthdaysByTime?month={DateTime.Now.Month}&day={DateTime.Now.Day}",
            "HOME_BIRTHDAYS_FAILED",
            warnings,
            cancellationToken);
        var announcementsResult = await GetListAsync<AnnouncementItemModel>("api/home/ListAnnouncements", "HOME_ANNOUNCEMENTS_FAILED", warnings, cancellationToken);
        var recentlyEditedGamesResult = await GetListAsync<RecentlyEditedGameItemModel>("api/home/ListRecentlyEditedGames", "HOME_RECENTLY_EDITED_GAMES_FAILED", warnings, cancellationToken);
        var evaluationsResult = await GetListAsync<EvaluationItemModel>("api/home/ListEvaluations", "HOME_EVALUATIONS_FAILED", warnings, cancellationToken);
        var hotTagsResult = await GetListAsync<HotTagItemModel>("api/home/ListHotTags", "HOME_HOT_TAGS_FAILED", warnings, cancellationToken);
        var latestCommentsResult = await GetListAsync<LatestCommentItemModel>("api/home/ListLatestComments?renderMarkdown=true", "HOME_LATEST_COMMENTS_FAILED", warnings, cancellationToken);
        var freeGamesResult = await GetListAsync<FreeGameItemModel>("api/home/ListFreeGames", "HOME_FREE_GAMES_FAILED", warnings, cancellationToken);
        var discountGamesResult = await GetListAsync<DiscountGameItemModel>("api/home/ListDiscountGames", "HOME_DISCOUNT_GAMES_FAILED", warnings, cancellationToken);

        if (warnings.Count == totalRequests)
        {
            return SdkResult<HomeSummaryViewModel>.Fail("HOME_ALL_REQUESTS_FAILED", "首页数据加载失败，请稍后重试");
        }

        var model = new HomeSummaryViewModel
        {
            HeroTitle = "CnGal 资料站",
            HeroSubtitle = "愿每一个 CnGal 创作者的作品都能不被忘记",
            Carousels = NormalizeCarouselLinks(carouselsResult),
            HotRecommends = NormalizeHomeItemUrls(hotRecommendsResult),
            PublishedGames = NormalizeHomeItemUrls(publishedGamesResult),
            RecentlyDemoGames = NormalizeHomeItemUrls(recentlyDemoGamesResult),
            UpcomingGames = NormalizeHomeItemUrls(upcomingGamesResult),
            ActivityCarousels = NormalizeCarouselLinks(activityCarouselsResult),
            HomeNews = NormalizeHomeNews(homeNewsResult),
            WeeklyNews = weeklyNewsResult,
            LatestArticles = NormalizeHomeItemUrls(latestArticlesResult),
            LatestVideos = NormalizeHomeItemUrls(latestVideosResult),
            FriendLinks = NormalizeHomeItemUrls(friendLinksResult),
            Birthdays = birthdaysResult,
            Announcements = NormalizeHomeItemUrls(announcementsResult),
            RecentlyEditedGames = NormalizeHomeItemUrls(recentlyEditedGamesResult),
            CommunityLinks = CreateCommunityLinks(),
            Evaluations = NormalizeHomeItemUrls(evaluationsResult),
            SupportLinks = CreateSupportLinks(),
            HotTags = NormalizeHomeItemUrls(hotTagsResult),
            LatestComments = NormalizeHomeItemUrls(latestCommentsResult),
            FreeGames = NormalizeHomeItemUrls(freeGamesResult),
            DiscountGames = NormalizeHomeItemUrls(discountGamesResult),
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

    private static IReadOnlyList<TItem> NormalizeHomeItemUrls<TItem>(IReadOnlyList<TItem> source)
        where TItem : HomeItemModel
    {
        return source.Select(s =>
        {
            s.Url = NormalizePath(s.Url);
            return s;
        }).ToList();
    }

    private static IReadOnlyList<CarouselViewModel> NormalizeCarouselLinks(IReadOnlyList<CarouselViewModel> source)
    {
        return source.Select(s =>
        {
            s.Link = NormalizePath(s.Link);
            return s;
        }).ToList();
    }

    private static IReadOnlyList<HomeNewsAloneViewModel> NormalizeHomeNews(IReadOnlyList<HomeNewsAloneViewModel> source)
    {
        return source.Select(s =>
        {
            s.Link = NormalizePath(s.Link);
            return s;
        }).ToList();
    }

    private static IReadOnlyList<HomeCommunityLinkViewModel> CreateCommunityLinks()
    {
        return
        [
            new()
            {
                Title = "CnGal玩家交流群",
                Text = "本群无资源请支持正版，调戏看板娘要注意节度哦",
                Link = "https://qm.qq.com/q/bgKC6yy5ri"
            },
            new()
            {
                Title = "CnGal编辑者交流与Bug反馈群",
                Text = "群主很懒,什么都没有留下",
                Link = "https://jq.qq.com/?_wv=1027&k=JzuI1IkF"
            },
            new()
            {
                Title = "QQ频道",
                Text = "CnGal资料站官方QQ频道",
                Link = "https://qun.qq.com/qqweb/qunpro/share?_wv=3&_wwv=128&inviteCode=onAQQ&from=246610&biz=ka"
            }
        ];
    }

    private static IReadOnlyList<HomeSupportLinkViewModel> CreateSupportLinks()
    {
        return
        [
            new()
            {
                Title = "为TA充电",
                Text = "CnGal 中文GalGame资料站 的日常运营",
                Link = "https://space.bilibili.com/145239325"
            }
        ];
    }

    private static string NormalizePath(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return "#";
        }

        if (url.StartsWith("//", StringComparison.Ordinal))
        {
            return $"https:{url}";
        }

        return url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : $"/{url.TrimStart('/')}";
    }
}
