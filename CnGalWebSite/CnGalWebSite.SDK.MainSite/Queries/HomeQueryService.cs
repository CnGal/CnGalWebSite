using System.Collections;
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

        var warnings = new ThreadSafeWarnings();
        const int totalRequests = 20;

        // 并行启动所有请求
        var tableTask = GetResultSafeAsync<TableViewModel>("api/tables/GetTableView", "HOME_TABLE_FAILED",
            warnings, cancellationToken);
        var carouselsTask = GetListSafeAsync<CarouselViewModel>("api/home/GetHomeCarouselsView",
            "HOME_CAROUSELS_FAILED", warnings, cancellationToken);
        var hotRecommendsTask = GetListSafeAsync<HotRecommendItemModel>("api/home/ListHotRecommends",
            "HOME_HOT_RECOMMENDS_FAILED", warnings, cancellationToken);
        var publishedGamesTask = GetListSafeAsync<PublishedGameItemModel>("api/home/ListPublishedGames",
            "HOME_PUBLISHED_GAMES_FAILED", warnings, cancellationToken);
        var recentlyDemoGamesTask = GetListSafeAsync<RecentlyDemoGameItemModel>(
            "api/home/ListRecentlyDemoGames", "HOME_RECENTLY_DEMO_GAMES_FAILED", warnings, cancellationToken);
        var upcomingGamesTask = GetListSafeAsync<UpcomingGameItemModel>("api/home/ListUpcomingGames",
            "HOME_UPCOMING_GAMES_FAILED", warnings, cancellationToken);
        var activityCarouselsTask = GetListSafeAsync<CarouselViewModel>("api/home/GetActivityCarouselsView",
            "HOME_ACTIVITY_CAROUSELS_FAILED", warnings, cancellationToken);
        var homeNewsTask = GetListSafeAsync<HomeNewsAloneViewModel>("api/home/GetHomeNewsView",
            "HOME_NEWS_FAILED", warnings, cancellationToken);
        var weeklyNewsTask = GetListSafeAsync<ArticleInforTipViewModel>("api/news/GetWeeklyNewsOverview",
            "HOME_WEEKLY_NEWS_FAILED", warnings, cancellationToken);
        var latestArticlesTask = GetListSafeAsync<LatestArticleItemModel>("api/home/ListLatestArticles",
            "HOME_LATEST_ARTICLES_FAILED", warnings, cancellationToken);
        var latestVideosTask = GetListSafeAsync<LatestVideoItemModel>("api/home/ListLatestVideos",
            "HOME_LATEST_VIDEOS_FAILED", warnings, cancellationToken);
        var friendLinksTask = GetListSafeAsync<FriendLinkItemModel>("api/home/ListFriendLinks",
            "HOME_FRIEND_LINKS_FAILED", warnings, cancellationToken);
        var birthdaysTask = GetListSafeAsync<RoleBrithdayViewModel>(
            $"api/entries/GetRoleBirthdaysByTime?month={DateTime.Now.Month}&day={DateTime.Now.Day}",
            "HOME_BIRTHDAYS_FAILED",
            warnings,
            cancellationToken);
        var announcementsTask = GetListSafeAsync<AnnouncementItemModel>("api/home/ListAnnouncements",
            "HOME_ANNOUNCEMENTS_FAILED", warnings, cancellationToken);
        var recentlyEditedGamesTask = GetListSafeAsync<RecentlyEditedGameItemModel>(
            "api/home/ListRecentlyEditedGames", "HOME_RECENTLY_EDITED_GAMES_FAILED", warnings, cancellationToken);
        var evaluationsTask = GetListSafeAsync<EvaluationItemModel>("api/home/ListEvaluations",
            "HOME_EVALUATIONS_FAILED", warnings, cancellationToken);
        var hotTagsTask = GetListSafeAsync<HotTagItemModel>("api/home/ListHotTags", "HOME_HOT_TAGS_FAILED",
            warnings, cancellationToken);
        var latestCommentsTask = GetListSafeAsync<LatestCommentItemModel>(
            "api/home/ListLatestComments?renderMarkdown=true", "HOME_LATEST_COMMENTS_FAILED", warnings,
            cancellationToken);
        var freeGamesTask = GetListSafeAsync<FreeGameItemModel>("api/home/ListFreeGames",
            "HOME_FREE_GAMES_FAILED", warnings, cancellationToken);
        var discountGamesTask = GetListSafeAsync<DiscountGameItemModel>("api/home/ListDiscountGames",
            "HOME_DISCOUNT_GAMES_FAILED", warnings, cancellationToken);

        // 等待所有请求完成
        await Task.WhenAll(
            tableTask, carouselsTask, hotRecommendsTask, publishedGamesTask,
            recentlyDemoGamesTask, upcomingGamesTask, activityCarouselsTask,
            homeNewsTask, weeklyNewsTask, latestArticlesTask, latestVideosTask,
            friendLinksTask, birthdaysTask, announcementsTask, recentlyEditedGamesTask,
            evaluationsTask, hotTagsTask, latestCommentsTask, freeGamesTask, discountGamesTask);

        if (warnings.Count == totalRequests)
        {
            return SdkResult<HomeSummaryViewModel>.Fail("HOME_ALL_REQUESTS_FAILED", "首页数据加载失败，请稍后重试");
        }

        var model = new HomeSummaryViewModel
        {
            HeroTitle = "CnGal 资料站",
            HeroSubtitle = "愿每一个 CnGal 创作者的作品都能不被忘记",
            Carousels = NormalizeCarouselLinks(carouselsTask.Result),
            HotRecommends = NormalizeHomeItemUrls(hotRecommendsTask.Result),
            PublishedGames = NormalizeHomeItemUrls(publishedGamesTask.Result),
            RecentlyDemoGames = NormalizeHomeItemUrls(recentlyDemoGamesTask.Result),
            UpcomingGames = NormalizeHomeItemUrls(upcomingGamesTask.Result),
            ActivityCarousels = NormalizeCarouselLinks(activityCarouselsTask.Result),
            HomeNews = NormalizeHomeNews(homeNewsTask.Result),
            WeeklyNews = weeklyNewsTask.Result,
            LatestArticles = NormalizeHomeItemUrls(latestArticlesTask.Result),
            LatestVideos = NormalizeHomeItemUrls(latestVideosTask.Result),
            FriendLinks = NormalizeHomeItemUrls(friendLinksTask.Result),
            Birthdays = birthdaysTask.Result,
            Announcements = NormalizeHomeItemUrls(announcementsTask.Result),
            RecentlyEditedGames = NormalizeHomeItemUrls(recentlyEditedGamesTask.Result),
            CommunityLinks = CreateCommunityLinks(),
            Evaluations = NormalizeHomeItemUrls(evaluationsTask.Result),
            SupportLink = CreateSupportLink(),
            HotTags = NormalizeHomeItemUrls(hotTagsTask.Result),
            LatestComments = NormalizeHomeItemUrls(latestCommentsTask.Result),
            FreeGames = NormalizeHomeItemUrls(freeGamesTask.Result),
            DiscountGames = NormalizeHomeItemUrls(discountGamesTask.Result),
            CommunityStats = tableTask.Result ?? new TableViewModel(),
            Warnings = warnings.ToList()
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

    /// <summary>
    /// 最小化的线程安全 ICollection 封装，用于并行请求中收集 warnings。
    /// </summary>
    private sealed class ThreadSafeWarnings : ICollection<SdkErrorModel>
    {
        private readonly List<SdkErrorModel> _list = [];
        private readonly Lock _lock = new();

        public int Count { get { lock (_lock) return _list.Count; } }
        public bool IsReadOnly => false;

        public void Add(SdkErrorModel item) { lock (_lock) _list.Add(item); }
        public List<SdkErrorModel> ToList() { lock (_lock) return [.. _list]; }

        // 以下方法仅为满足接口约束，并行聚合场景不使用
        public void Clear() { lock (_lock) _list.Clear(); }
        public bool Contains(SdkErrorModel item) { lock (_lock) return _list.Contains(item); }
        public void CopyTo(SdkErrorModel[] array, int arrayIndex) { lock (_lock) _list.CopyTo(array, arrayIndex); }
        public bool Remove(SdkErrorModel item) { lock (_lock) return _list.Remove(item); }
        public IEnumerator<SdkErrorModel> GetEnumerator() { lock (_lock) return ToList().GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
