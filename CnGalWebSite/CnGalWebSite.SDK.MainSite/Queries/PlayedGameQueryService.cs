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
    private const string OverviewPathTemplate = "api/playedgame/GetPlayedGameOverview/{0}";
    private const string UserRecordsPathTemplate = "api/playedgame/GetUserGameRecords/{0}";
    private const string UserSteamInfoPathTemplate = "api/steam/GetUserSteamInfor/{0}";

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
}

