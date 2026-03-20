using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.SpaceEdit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class SpaceCommandService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<SpaceCommandService> logger) : CommandServiceBase(httpClient), ISpaceCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<SpaceEditViewModel>> GetSpaceEditAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // 并发获取个人资料和个人主页数据
            var userDataTask = GetFromJsonAsync<EditUserDataViewModel>("api/space/edituserdata", cancellationToken);
            var mainPageTask = GetFromJsonAsync<EditUserMainPageViewModel>("api/space/editmainpage", cancellationToken);

            await Task.WhenAll(userDataTask, mainPageTask);

            var model = new SpaceEditViewModel
            {
                UserData = userDataTask.Result ?? new(),
                MainPage = mainPageTask.Result ?? new()
            };

            return SdkResult<SpaceEditViewModel>.Ok(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取用户编辑数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<SpaceEditViewModel>.Fail("SPACE_EDIT_QUERY_EXCEPTION", "请求用户编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<string>> SubmitEditAsync(SpaceEditViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var errors = new List<string>();

            // 顺序提交：先保存个人资料（含 SteamId 等），再保存个人主页，避免并发导致数据覆盖
            var userDataResult = await PostAsJsonAsync<EditUserDataViewModel, Result>("api/space/edituserdata", model.UserData, cancellationToken);
            if (userDataResult is { Successful: false })
            {
                errors.Add(userDataResult.Error ?? "保存个人资料失败");
            }

            var mainPageResult = await PostAsJsonAsync<EditUserMainPageViewModel, Result>("api/space/editmainpage", model.MainPage, cancellationToken);
            if (mainPageResult is { Successful: false })
            {
                errors.Add(mainPageResult.Error ?? "保存个人主页失败");
            }

            if (errors.Count > 0)
            {
                return SdkResult<string>.Fail("SPACE_EDIT_SUBMIT_FAILED", string.Join("；", errors));
            }

            // 清空对应用户的空间详情、游玩记录、Steam 信息缓存
            var userId = model.UserData.Id ?? string.Empty;
            InvalidateUserCaches(userId);

            // 返回用户 ID 以供跳转
            return SdkResult<string>.Ok(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交用户编辑数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("SPACE_EDIT_SUBMIT_EXCEPTION", "提交用户编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<string>> RefreshSteamInfoAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>("api/playedgame/RefreshPlayedGameSteamInfor", cancellationToken);
            if (result is { Successful: true })
            {
                // 清空相关缓存，确保后续查询获取最新数据
                if (!string.IsNullOrEmpty(userId))
                {
                    InvalidateUserCaches(userId);
                }

                return SdkResult<string>.Ok("刷新成功");
            }

            return SdkResult<string>.Fail("STEAM_REFRESH_FAILED", result?.Error ?? "刷新 Steam 信息失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刷新 Steam 信息异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("STEAM_REFRESH_EXCEPTION", "刷新 Steam 信息时发生异常");
        }
    }

    private void InvalidateUserCaches(string userId)
    {
        memoryCache.Remove($"main-site:space-detail:{userId}");
        memoryCache.Remove($"main-site:user-game-records:{userId}");
        memoryCache.Remove($"main-site:user-steam-info:{userId}");
    }
}
