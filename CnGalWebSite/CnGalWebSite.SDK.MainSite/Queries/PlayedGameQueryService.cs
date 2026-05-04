using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class PlayedGameQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<PlayedGameQueryService> logger) : QueryServiceBase(httpClient), IPlayedGameQueryService
{
    private static readonly TimeSpan OverviewCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RecordsCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SteamInfoCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan GamesOverviewCacheDuration = TimeSpan.FromMinutes(10);
    private const string OverviewPathTemplate = "api/playedgame/GetPlayedGameOverview/{0}";
    private const string UserRecordsPathTemplate = "api/playedgame/GetUserGameRecords/{0}";
    private const string UserSteamInfoPathTemplate = "api/steam/GetUserSteamInfor/{0}";
    private const string GamesOverviewPath = "api/steam/GetSteamGamesOverview";
    private const string RandomReviewsPath = "api/playedgame/GetRandomUserScores";

    protected override ILogger Logger => logger;

    public async Task<SdkResult<PlayedGameOverviewViewModel>> GetOverviewAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:played-game-overview:{entryId}";
        if (memoryCache.TryGetValue(cacheKey, out PlayedGameOverviewViewModel? cached) && cached is not null)
        {
            return SdkResult<PlayedGameOverviewViewModel>.Ok(cached);
        }

        var path = string.Format(OverviewPathTemplate, entryId);
        var result = await GetSingleAsync<PlayedGameOverviewModel, PlayedGameOverviewViewModel>(
            path,
            MapOverviewToViewModel,
            "PLAYED_GAME",
            "评分概览",
            entryId,
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, OverviewCacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<UserGameRecordsViewModel>> GetUserGameRecordsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:user-game-records:{userId}";
        if (memoryCache.TryGetValue(cacheKey, out UserGameRecordsViewModel? cached) && cached is not null)
        {
            return SdkResult<UserGameRecordsViewModel>.Ok(cached);
        }

        var path = string.Format(UserRecordsPathTemplate, userId);
        var result = await GetAsync<List<GameRecordViewModel>>(
            path,
            "USER_GAME_RECORDS",
            "用户游玩记录",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<UserGameRecordsViewModel>.Fail(
                result.Error?.Code ?? "USER_GAME_RECORDS_FAILED",
                result.Error?.Message ?? "获取用户游玩记录失败",
                result.Error?.StatusCode);
        }

        var viewModel = MapRecordsToViewModel(result.Data);
        memoryCache.Set(cacheKey, viewModel, RecordsCacheDuration);
        return SdkResult<UserGameRecordsViewModel>.Ok(viewModel);
    }

    public async Task<SdkResult<IReadOnlyList<SteamUserInfoItem>>> GetUserSteamInfoAsync(string userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:user-steam-info:{userId}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<SteamUserInfoItem>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<SteamUserInfoItem>>.Ok(cached);
        }

        var path = string.Format(UserSteamInfoPathTemplate, userId);
        var result = await GetAsync<List<SteamUserInforModel>>(
            path,
            "USER_STEAM_INFO",
            "用户Steam信息",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            // Steam 信息获取失败时返回空列表而非错误，保证页面可渲染
            return SdkResult<IReadOnlyList<SteamUserInfoItem>>.Ok(Array.Empty<SteamUserInfoItem>());
        }

        IReadOnlyList<SteamUserInfoItem> items = result.Data
            .Select(s => new SteamUserInfoItem
            {
                SteamId = s.SteamId ?? string.Empty,
                Name = s.Name ?? string.Empty,
                Image = s.Image ?? string.Empty,
                Price = s.Price,
            })
            .ToList();

        memoryCache.Set(cacheKey, items, SteamInfoCacheDuration);
        return SdkResult<IReadOnlyList<SteamUserInfoItem>>.Ok(items);
    }

    private static PlayedGameOverviewViewModel MapOverviewToViewModel(PlayedGameOverviewModel model)
    {
        return new PlayedGameOverviewViewModel
        {
            GameName = model.Game?.Name ?? string.Empty,
            GameId = model.Game?.Id ?? 0,
            IsDubbing = model.IsDubbing,
            IsCurrentUserScoreExist = model.IsCurrentUserScoreExist,
            IsCurrentUserScorePublic = model.IsCurrentUserScorePublic,
            CurrentUserId = model.CurrentUserId,
            TotalScores = MapScoreSummary(model.GameTotalScores),
            FilteredScores = MapScoreSummary(model.GameReviewsScores),
            UserScores = model.UserScores?
                .Where(s => s.Socres is not null && s.User is not null)
                .Select(s => new UserScoreItem
                {
                    UserId = s.User.Id ?? string.Empty,
                    UserName = s.User.Name ?? string.Empty,
                    UserPhoto = s.User.PhotoPath ?? string.Empty,
                    Ranks = s.User.Ranks?.ToList() ?? [],
                    MusicScore = s.Socres.MusicSocre,
                    PaintScore = s.Socres.PaintSocre,
                    ScriptScore = s.Socres.ScriptSocre,
                    ShowScore = s.Socres.ShowSocre,
                    SystemScore = s.Socres.SystemSocre,
                    CVScore = s.Socres.CVSocre,
                    TotalScore = s.Socres.TotalSocre,
                    PlayImpressions = s.PlayImpressions,
                    LastEditTime = s.LastEditTime
                })
                .ToList() ?? []
        };
    }

    private static ScoreSummary MapScoreSummary(PlayedGameScoreModel? scores)
    {
        if (scores is null)
        {
            return new ScoreSummary
            {
                MusicScore = 0, PaintScore = 0, ScriptScore = 0,
                ShowScore = 0, SystemScore = 0, CVScore = 0, TotalScore = 0
            };
        }

        return new ScoreSummary
        {
            MusicScore = scores.MusicSocre,
            PaintScore = scores.PaintSocre,
            ScriptScore = scores.ScriptSocre,
            ShowScore = scores.ShowSocre,
            SystemScore = scores.SystemSocre,
            CVScore = scores.CVSocre,
            TotalScore = scores.TotalSocre
        };
    }

    private static UserGameRecordsViewModel MapRecordsToViewModel(List<GameRecordViewModel> records)
    {
        return new UserGameRecordsViewModel
        {
            Records = records
                .Select(r => new GameRecordItem
                {
                    GameId = r.GameId,
                    GameName = r.GameName ?? string.Empty,
                    GameImage = r.GameImage ?? string.Empty,
                    GameBriefIntroduction = r.GameBriefIntroduction ?? string.Empty,
                    Type = r.Type,
                    PlayDuration = r.PlayDuration,
                    IsInSteam = r.IsInSteam,
                    IsDubbing = r.IsDubbing,
                    MusicScore = r.MusicSocre,
                    PaintScore = r.PaintSocre,
                    ScriptScore = r.ScriptSocre,
                    ShowScore = r.ShowSocre,
                    SystemScore = r.SystemSocre,
                    CVScore = r.CVSocre,
                    TotalScore = r.TotalSocre,
                    PlayImpressions = r.PlayImpressions,
                    ShowPublicly = r.ShowPublicly,
                    IsHidden = r.IsHidden
                })
                .ToList()
        };
    }

    public async Task<SdkResult<SteamGamesOverviewViewModel>> GetSteamGamesOverviewAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:steam-games-overview";
        if (memoryCache.TryGetValue(cacheKey, out SteamGamesOverviewViewModel? cached) && cached is not null)
        {
            return SdkResult<SteamGamesOverviewViewModel>.Ok(cached);
        }

        var result = await GetAsync<SteamGamesOverviewModel>(
            GamesOverviewPath,
            "STEAM_GAMES_OVERVIEW",
            "Steam游戏总览",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<SteamGamesOverviewViewModel>.Fail(
                result.Error?.Code ?? "STEAM_GAMES_OVERVIEW_FAILED",
                result.Error?.Message ?? "获取 Steam 游戏总览失败",
                result.Error?.StatusCode);
        }

        var viewModel = MapGamesOverviewToViewModel(result.Data);
        memoryCache.Set(cacheKey, viewModel, GamesOverviewCacheDuration);
        return SdkResult<SteamGamesOverviewViewModel>.Ok(viewModel);
    }

    public void InvalidateUserRecordsCache(string userId)
    {
        var cacheKey = $"main-site:user-game-records:{userId}";
        memoryCache.Remove(cacheKey);
    }

    public void InvalidateOverviewCache(int entryId)
    {
        var cacheKey = $"main-site:played-game-overview:{entryId}";
        memoryCache.Remove(cacheKey);
    }

    public async Task<SdkResult<IReadOnlyList<RandomReviewItem>>> GetRandomReviewsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:random-reviews";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<RandomReviewItem>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<RandomReviewItem>>.Ok(cached);
        }

        var result = await GetAsync<List<PlayedGameUserScoreRandomModel>>(
            RandomReviewsPath,
            "RANDOM_REVIEWS",
            "随机用户评价",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<RandomReviewItem>>.Ok(Array.Empty<RandomReviewItem>());
        }

        IReadOnlyList<RandomReviewItem> items = result.Data
            .Select(MapRandomReview)
            .ToList();

        memoryCache.Set(cacheKey, items, TimeSpan.FromMinutes(2));
        return SdkResult<IReadOnlyList<RandomReviewItem>>.Ok(items);
    }

    private static SteamGamesOverviewViewModel MapGamesOverviewToViewModel(SteamGamesOverviewModel model)
    {
        return new SteamGamesOverviewViewModel
        {
            TotalGameCount = model.Count,
            TopUsers = model.HasMostGamesUsers
                .Select(u => new TopUserItem
                {
                    UserId = u.Id ?? string.Empty,
                    UserName = u.Name ?? string.Empty,
                    UserImage = u.Image ?? string.Empty,
                    PersonalSignature = u.PersonalSignature ?? string.Empty,
                    GameCount = u.Count,
                    Ranks = u.Ranks?.ToList() ?? [],
                })
                .ToList(),
            TopGames = model.PossessionRateHighestGames
                .Select(g => new TopGameItem
                {
                    GameId = g.Id,
                    GameName = g.Name ?? string.Empty,
                    GameImage = g.MainImage ?? string.Empty,
                    PossessionRate = g.Rate,
                })
                .ToList(),
        };
    }

    private static RandomReviewItem MapRandomReview(PlayedGameUserScoreRandomModel model)
    {
        return new RandomReviewItem
        {
            UserId = model.User?.Id ?? string.Empty,
            UserName = model.User?.Name ?? string.Empty,
            UserImage = model.User?.PhotoPath ?? string.Empty,
            Ranks = model.User?.Ranks?.ToList() ?? [],
            GameId = model.GameId,
            GameName = model.GameName ?? string.Empty,
            TotalScore = model.Socres?.TotalSocre ?? 0,
            MusicScore = model.Socres?.MusicSocre ?? 0,
            PaintScore = model.Socres?.PaintSocre ?? 0,
            ScriptScore = model.Socres?.ScriptSocre ?? 0,
            ShowScore = model.Socres?.ShowSocre ?? 0,
            SystemScore = model.Socres?.SystemSocre ?? 0,
            CVScore = model.Socres?.CVSocre ?? 0,
            IsDubbing = model.IsDubbing,
            PlayImpressions = model.PlayImpressions ?? string.Empty,
            LastEditTime = model.LastEditTime,
        };
    }
}
