using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class PlayedGameCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PlayedGameCommandService> logger) : CommandServiceBase(httpClient), IPlayedGameCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<EditGameRecordModel>> GetEditDataAsync(int gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await GetFromJsonAsync<EditGameRecordModel>($"api/playedgame/EditGameRecord/{gameId}", cancellationToken);
            if (model is null)
            {
                return SdkResult<EditGameRecordModel>.Fail("PLAYED_GAME_EDIT_DATA_NULL", "获取编辑数据为空");
            }

            return SdkResult<EditGameRecordModel>.Ok(model);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "获取游玩记录编辑数据失败。GameId={GameId}; StatusCode={StatusCode}; BaseAddress={BaseAddress}",
                gameId, ex.StatusCode, HttpClient.BaseAddress);
            return SdkResult<EditGameRecordModel>.Fail("PLAYED_GAME_EDIT_DATA_HTTP_FAILED", "获取游玩记录编辑数据失败", (int?)ex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取游玩记录编辑数据异常。GameId={GameId}; BaseAddress={BaseAddress}", gameId, HttpClient.BaseAddress);
            return SdkResult<EditGameRecordModel>.Fail("PLAYED_GAME_EDIT_DATA_EXCEPTION", "获取游玩记录编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<bool>> SaveAsync(EditGameRecordModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<EditGameRecordModel, Result>("api/playedgame/EditGameRecord", model, cancellationToken);
            if (result is null)
            {
                return SdkResult<bool>.Fail("PLAYED_GAME_SAVE_NULL", "保存游玩记录返回结果为空");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("PLAYED_GAME_SAVE_FAILED", result.Error ?? "保存游玩记录失败");
            }

            InvalidatePlayedGameCaches(model.GameId);
            return SdkResult<bool>.Ok(true);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "保存游玩记录失败。GameId={GameId}; StatusCode={StatusCode}; BaseAddress={BaseAddress}",
                model.GameId, ex.StatusCode, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PLAYED_GAME_SAVE_HTTP_FAILED", "保存游玩记录失败", (int?)ex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "保存游玩记录异常。GameId={GameId}; BaseAddress={BaseAddress}", model.GameId, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PLAYED_GAME_SAVE_EXCEPTION", "保存游玩记录时发生异常");
        }
    }

    public async Task<SdkResult<bool>> ToggleHiddenAsync(int gameId, bool isHidden, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HiddenGameRecordModel
            {
                GameIds = [gameId],
                IsHidden = isHidden
            };

            var result = await PostAsJsonAsync<HiddenGameRecordModel, Result>("api/playedgame/HiddenGameRecord", request, cancellationToken);
            if (result is null)
            {
                return SdkResult<bool>.Fail("PLAYED_GAME_HIDDEN_NULL", "修改隐藏状态返回结果为空");
            }

            if (!result.Successful)
            {
                return SdkResult<bool>.Fail("PLAYED_GAME_HIDDEN_FAILED", result.Error ?? "修改隐藏状态失败");
            }

            InvalidatePlayedGameCaches(gameId);
            return SdkResult<bool>.Ok(true);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "修改游玩记录隐藏状态失败。GameId={GameId}; StatusCode={StatusCode}; BaseAddress={BaseAddress}",
                gameId, ex.StatusCode, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PLAYED_GAME_HIDDEN_HTTP_FAILED", "修改隐藏状态失败", (int?)ex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "修改游玩记录隐藏状态异常。GameId={GameId}; BaseAddress={BaseAddress}", gameId, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PLAYED_GAME_HIDDEN_EXCEPTION", "修改隐藏状态时发生异常");
        }
    }

    private void InvalidatePlayedGameCaches(int gameId)
    {
        memoryCache.Remove($"main-site:played-game-overview:{gameId}");
        var userId = httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            memoryCache.Remove($"main-site:user-game-records:{userId}");
        }
    }
}
