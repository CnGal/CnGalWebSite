using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tables;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Votes;
using CnGalWebSite.Extensions;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class HomeQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<HomeQueryService> logger) : QueryServiceBase(httpClient), IHomeQueryService
{
    private static readonly TimeSpan HomeCacheDuration = TimeSpan.FromMinutes(2);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<HomeSummaryViewModel>> GetHomeSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:home-summary";
        if (memoryCache.TryGetValue(cacheKey, out HomeSummaryViewModel? cached) && cached is not null)
        {
            return SdkResult<HomeSummaryViewModel>.Ok(cached);
        }

        var warnings = new List<SdkErrorModel>();
        const int totalRequests = 20;

        var tableResult = await GetResultSafeAsync<TableViewModel>("api/tables/GetTableView", "HOME_TABLE_FAILED",
            warnings, cancellationToken);

        var carouselsResult = await GetListSafeAsync<CarouselViewModel>("api/home/GetHomeCarouselsView",
            "HOME_CAROUSELS_FAILED", warnings, cancellationToken);
        var hotRecommendsResult = await GetListSafeAsync<HotRecommendItemModel>("api/home/ListHotRecommends",
            "HOME_HOT_RECOMMENDS_FAILED", warnings, cancellationToken);
        var publishedGamesResult = await GetListSafeAsync<PublishedGameItemModel>("api/home/ListPublishedGames",
            "HOME_PUBLISHED_GAMES_FAILED", warnings, cancellationToken);
        var recentlyDemoGamesResult = await GetListSafeAsync<RecentlyDemoGameItemModel>(
            "api/home/ListRecentlyDemoGames", "HOME_RECENTLY_DEMO_GAMES_FAILED", warnings, cancellationToken);
        var upcomingGamesResult = await GetListSafeAsync<UpcomingGameItemModel>("api/home/ListUpcomingGames",
            "HOME_UPCOMING_GAMES_FAILED", warnings, cancellationToken);
        var activityCarouselsResult = await GetListSafeAsync<CarouselViewModel>("api/home/GetActivityCarouselsView",
            "HOME_ACTIVITY_CAROUSELS_FAILED", warnings, cancellationToken);
        var homeNewsResult = await GetListSafeAsync<HomeNewsAloneViewModel>("api/home/GetHomeNewsView",
            "HOME_NEWS_FAILED", warnings, cancellationToken);
        var weeklyNewsResult = await GetListSafeAsync<ArticleInforTipViewModel>("api/news/GetWeeklyNewsOverview",
            "HOME_WEEKLY_NEWS_FAILED", warnings, cancellationToken);
        var latestArticlesResult = await GetListSafeAsync<LatestArticleItemModel>("api/home/ListLatestArticles",
            "HOME_LATEST_ARTICLES_FAILED", warnings, cancellationToken);
        var latestVideosResult = await GetListSafeAsync<LatestVideoItemModel>("api/home/ListLatestVideos",
            "HOME_LATEST_VIDEOS_FAILED", warnings, cancellationToken);
        var friendLinksResult = await GetListSafeAsync<FriendLinkItemModel>("api/home/ListFriendLinks",
            "HOME_FRIEND_LINKS_FAILED", warnings, cancellationToken);
        var birthdaysResult = await GetListSafeAsync<RoleBrithdayViewModel>(
            $"api/entries/GetRoleBirthdaysByTime?month={DateTime.Now.Month}&day={DateTime.Now.Day}",
            "HOME_BIRTHDAYS_FAILED",
            warnings,
            cancellationToken);
        var announcementsResult = await GetListSafeAsync<AnnouncementItemModel>("api/home/ListAnnouncements",
            "HOME_ANNOUNCEMENTS_FAILED", warnings, cancellationToken);
        var recentlyEditedGamesResult = await GetListSafeAsync<RecentlyEditedGameItemModel>(
            "api/home/ListRecentlyEditedGames", "HOME_RECENTLY_EDITED_GAMES_FAILED", warnings, cancellationToken);
        var evaluationsResult = await GetListSafeAsync<EvaluationItemModel>("api/home/ListEvaluations",
            "HOME_EVALUATIONS_FAILED", warnings, cancellationToken);
        var hotTagsResult = await GetListSafeAsync<HotTagItemModel>("api/home/ListHotTags", "HOME_HOT_TAGS_FAILED",
            warnings, cancellationToken);
        var latestCommentsResult = await GetListSafeAsync<LatestCommentItemModel>(
            "api/home/ListLatestComments?renderMarkdown=true", "HOME_LATEST_COMMENTS_FAILED", warnings,
            cancellationToken);
        var freeGamesResult = await GetListSafeAsync<FreeGameItemModel>("api/home/ListFreeGames",
            "HOME_FREE_GAMES_FAILED", warnings, cancellationToken);
        var discountGamesResult = await GetListSafeAsync<DiscountGameItemModel>("api/home/ListDiscountGames",
            "HOME_DISCOUNT_GAMES_FAILED", warnings, cancellationToken);

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
            SupportLink = CreateSupportLink(),
            HotTags = NormalizeHomeItemUrls(hotTagsResult),
            LatestComments = NormalizeHomeItemUrls(latestCommentsResult),
            FreeGames = NormalizeHomeItemUrls(freeGamesResult),
            DiscountGames = NormalizeHomeItemUrls(discountGamesResult),
            CommunityStats = tableResult ?? new TableViewModel(),
            Warnings = warnings
        };

        memoryCache.Set(cacheKey, model, HomeCacheDuration);
        return SdkResult<HomeSummaryViewModel>.Ok(model);
    }

    public async Task<SdkResult<SearchViewModel>> SearchAsync(string queryString,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:search:{queryString}";
        if (memoryCache.TryGetValue(cacheKey, out SearchViewModel? cached) && cached is not null)
        {
            return SdkResult<SearchViewModel>.Ok(cached);
        }

        var path = $"api/home/search{queryString}";
        var result = await GetAsync<SearchViewModel>(path, "SEARCH", "搜索结果", cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, HomeCacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<SquareSummaryViewModel>> GetSquareSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:square-summary";
        if (memoryCache.TryGetValue(cacheKey, out SquareSummaryViewModel? cached) && cached is not null)
        {
            return SdkResult<SquareSummaryViewModel>.Ok(cached);
        }

        var warnings = new List<SdkErrorModel>();
        const int totalRequests = 4;

        var randomTagsResult = await GetListSafeAsync<RandomTagModel>("api/tags/GetRandomTags",
            "SQUARE_RANDOM_TAGS_FAILED", warnings, cancellationToken);
        var lotteriesResult = await GetListSafeAsync<LotteryCardViewModel>("api/lotteries/GetLotteryCards",
            "SQUARE_LOTTERIES_FAILED", warnings, cancellationToken);
        var votesResult = await GetListSafeAsync<VoteCardViewModel>("api/votes/GetVoteCards", "SQUARE_VOTES_FAILED",
            warnings, cancellationToken);

        // 编辑概览折线图（公开接口，无需授权）
        var editOverview = await GetEditOverviewChartSafeAsync(warnings, cancellationToken);

        if (warnings.Count == totalRequests)
        {
            return SdkResult<SquareSummaryViewModel>.Fail("SQUARE_ALL_REQUESTS_FAILED", "广场数据加载失败，请稍后重试");
        }

        var model = new SquareSummaryViewModel
        {
            RandomTags = randomTagsResult.Select(tag => new SquareRandomTagModel
            {
                Id = tag.Id,
                Name = tag.Name ?? string.Empty,
                Entries = tag.Entries?.Select(e => new SquareRandomEntryModel
                {
                    Id = e.Id,
                    Name = e.Name ?? string.Empty,
                    MainImage = e.MainImage ?? string.Empty,
                    Type = e.Type,
                }).ToList() ?? [],
            }).ToList(),
            Lotteries = lotteriesResult.Select(dto =>
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
                    ConditionType = dto.ConditionType,
                    IsEnd = dto.EndTime < now,
                    GameSteamId = dto.GameSteamId ?? string.Empty,
                };
            }).ToList(),
            Votes = votesResult.Select(dto =>
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
            }).ToList(),
            Warnings = warnings,
            EditOverview = editOverview,
        };

        memoryCache.Set(cacheKey, model, HomeCacheDuration);
        return SdkResult<SquareSummaryViewModel>.Ok(model);
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
            new() { Title = "CnGal玩家交流群", Text = "本群无资源请支持正版，调戏看板娘要注意节度哦", Link = "https://qm.qq.com/q/bgKC6yy5ri" },
            new()
            {
                Title = "CnGal编辑者交流与Bug反馈群", Text = "群主很懒,什么都没有留下", Link = "https://jq.qq.com/?_wv=1027&k=JzuI1IkF"
            },
            new()
            {
                Title = "QQ频道",
                Text = "CnGal资料站官方QQ频道",
                Link = "https://qun.qq.com/qqweb/qunpro/share?_wv=3&_wwv=128&inviteCode=onAQQ&from=246610&biz=ka"
            },
            new() { Title = "Bilibili", Text = "B站官方账号", Link = "https://space.bilibili.com/145239325" },
            new() { Title = "微博", Text = "微博官方账号", Link = "https://weibo.com/cngalorg" },
            new() { Title = "小黑盒", Text = "小黑盒官方账号", Link = "https://www.xiaoheihe.cn/app/user/profile/30519991" }
        ];
    }

    private static HomeSupportLinkViewModel CreateSupportLink()
    {
        return new HomeSupportLinkViewModel
        {
            Title = "为TA充电",
            Text = "CnGal 中文GalGame资料站 的日常运营",
            Link = "https://space.bilibili.com/145239325"
        };
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

    /// <summary>
    /// 安全获取编辑概览折线图数据，失败时返回 null 并记录 warning。
    /// </summary>
    private async Task<SquareEditOverviewModel?> GetEditOverviewChartSafeAsync(
        List<SdkErrorModel> warnings, CancellationToken cancellationToken)
    {
        try
        {
            var beforeTime = DateTime.Now.Date.AddDays(1);
            var afterTime = beforeTime.AddDays(-30);
            var afterMs = afterTime.ToUnixTimeMilliseconds();
            var beforeMs = beforeTime.ToUnixTimeMilliseconds();

            var result = await GetAsync<LineChartModel>(
                $"api/perfections/GetPerfectionLineChart?type=Edit&afterTime={afterMs}&beforeTime={beforeMs}",
                "SQUARE_EDIT_OVERVIEW",
                "编辑概览图表",
                cancellationToken);

            if (!result.Success || result.Data is null)
            {
                warnings.Add(new SdkErrorModel
                {
                    Code = "SQUARE_EDIT_OVERVIEW_FAILED",
                    Message = result.Error?.Message ?? "编辑概览图表加载失败",
                });
                return null;
            }

            var chart = result.Data;
            return new SquareEditOverviewModel
            {
                Labels = chart.Options.XAxis.Data,
                Series = chart.Options.Series.Select(s => new SquareEditOverviewSeriesModel
                {
                    Name = s.Name,
                    Data = s.Data,
                }).ToList(),
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "广场页编辑概览图表加载异常");
            warnings.Add(new SdkErrorModel { Code = "SQUARE_EDIT_OVERVIEW_EXCEPTION", Message = "编辑概览图表加载异常", });
            return null;
        }
    }
}
