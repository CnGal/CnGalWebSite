using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.SpaceEdit;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class SpaceCommandService(
    HttpClient httpClient,
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

            // 并发提交个人资料和个人主页
            var userDataTask = PostAsJsonAsync<EditUserDataViewModel, Result>("api/space/edituserdata", model.UserData, cancellationToken);
            var mainPageTask = PostAsJsonAsync<EditUserMainPageViewModel, Result>("api/space/editmainpage", model.MainPage, cancellationToken);

            await Task.WhenAll(userDataTask, mainPageTask);

            var userDataResult = userDataTask.Result;
            if (userDataResult is { Successful: false })
            {
                errors.Add(userDataResult.Error ?? "保存个人资料失败");
            }

            var mainPageResult = mainPageTask.Result;
            if (mainPageResult is { Successful: false })
            {
                errors.Add(mainPageResult.Error ?? "保存个人主页失败");
            }

            if (errors.Count > 0)
            {
                return SdkResult<string>.Fail("SPACE_EDIT_SUBMIT_FAILED", string.Join("；", errors));
            }

            // 返回用户 ID 以供跳转
            return SdkResult<string>.Ok(model.UserData.Id ?? string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交用户编辑数据异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("SPACE_EDIT_SUBMIT_EXCEPTION", "提交用户编辑数据时发生异常");
        }
    }

    public async Task<SdkResult<string>> RefreshSteamInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>("api/playedgame/RefreshPlayedGameSteamInfor", cancellationToken);
            if (result is { Successful: true })
            {
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
}
