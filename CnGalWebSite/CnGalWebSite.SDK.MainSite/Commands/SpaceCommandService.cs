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
            var result = await PostAsJsonAsync<object, Result>("api/playedgame/RefreshPlayedGameSteamInfor", null!, cancellationToken);
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

    public async Task<SdkResult<string>> ReadAllMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<object, Result>("api/space/ReadedAllMessages/", null!, cancellationToken);

            if (result is { Successful: true })
            {
                return SdkResult<string>.Ok("已将所有消息标记为已读");
            }

            return SdkResult<string>.Fail("MESSAGE_READ_ALL_FAILED", result?.Error ?? "标记消息已读失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "标记所有消息已读异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("MESSAGE_READ_ALL_EXCEPTION", "标记消息已读时发生异常");
        }
    }

    public async Task<SdkResult<string>> SignInAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<object, Result>("api/space/signIn", null!, cancellationToken);

            if (result is { Successful: true })
            {
                return SdkResult<string>.Ok("签到成功");
            }

            return SdkResult<string>.Fail("SIGN_IN_FAILED", result?.Error ?? "签到失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "每日签到异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("SIGN_IN_EXCEPTION", "签到时发生异常");
        }
    }

    public async Task<SdkResult<string>> MakeUserOnlineAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>("api/account/MakeUserOnline", cancellationToken);

            if (result is { Successful: true })
            {
                return SdkResult<string>.Ok("在线状态已更新");
            }

            return SdkResult<string>.Fail("MAKE_ONLINE_FAILED", result?.Error ?? "更新在线状态失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新在线状态异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("MAKE_ONLINE_EXCEPTION", "更新在线状态时发生异常");
        }
    }

    public async Task<SdkResult<EditUserAddressModel>> GetUserAddressAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var model = await GetFromJsonAsync<EditUserAddressModel>("api/space/EditUserAddress", cancellationToken);
            return SdkResult<EditUserAddressModel>.Ok(model ?? new EditUserAddressModel());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取用户地址异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<EditUserAddressModel>.Fail("ADDRESS_QUERY_EXCEPTION", "获取用户地址时发生异常");
        }
    }

    public async Task<SdkResult<bool>> EditUserAddressAsync(EditUserAddressModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<EditUserAddressModel, Result>("api/space/EditUserAddress", model, cancellationToken);
            if (result is { Successful: true })
            {
                return SdkResult<bool>.Ok(true);
            }

            return SdkResult<bool>.Fail("ADDRESS_EDIT_FAILED", result?.Error ?? "保存地址失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "保存用户地址异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("ADDRESS_EDIT_EXCEPTION", "保存用户地址时发生异常");
        }
    }

    public async Task<SdkResult<string>> GetBindGroupQQCodeAsync(UnBindGroupQQModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<UnBindGroupQQModel, Result>("api/space/GetBindGroupQQCode", model, cancellationToken);
            if (result is { Successful: true })
            {
                // 身份识别码通过 Result.Error 字段返回（历史原因）
                return SdkResult<string>.Ok(result.Error ?? string.Empty);
            }

            return SdkResult<string>.Fail("QQ_BIND_CODE_FAILED", result?.Error ?? "获取身份识别码失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取绑定群聊 QQ 身份识别码异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("QQ_BIND_CODE_EXCEPTION", "获取身份识别码时发生异常");
        }
    }

    public async Task<SdkResult<string>> UnBindGroupQQAsync(UnBindGroupQQModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<UnBindGroupQQModel, Result>("api/space/UnBindGroupQQ", model, cancellationToken);
            if (result is { Successful: true })
            {
                return SdkResult<string>.Ok("解除绑定成功");
            }

            return SdkResult<string>.Fail("QQ_UNBIND_FAILED", result?.Error ?? "解除绑定失败");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "解除群聊 QQ 绑定异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("QQ_UNBIND_EXCEPTION", "解除绑定时发生异常");
        }
    }

    public async Task<SdkResult<string>> EditUserCertificationAsync(EditUserCertificationModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<EditUserCertificationModel, Result>("api/space/EditUserCertification", model, cancellationToken);
            if (result is { Successful: true })
            {
                var message = string.IsNullOrWhiteSpace(model.EntryName) ? "已取消认证" : "已提交用户认证申请，等待审核通过";
                return SdkResult<string>.Ok(message);
            }

            var errorTitle = string.IsNullOrWhiteSpace(model.EntryName) ? "取消认证失败" : "提交用户认证申请失败";
            return SdkResult<string>.Fail("CERTIFICATION_EDIT_FAILED", result?.Error ?? errorTitle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "提交用户认证申请异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<string>.Fail("CERTIFICATION_EDIT_EXCEPTION", "提交用户认证申请时发生异常");
        }
    }

    public async Task<SdkResult<List<string>>> GetAllNotCertificatedEntriesAsync(EntryType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<List<string>>($"api/space/GetAllNotCertificatedEntries?type={type}", cancellationToken);
            return SdkResult<List<string>>.Ok(result ?? []);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "获取未认证词条列表异常。Type={Type}; BaseAddress={BaseAddress}", type, HttpClient.BaseAddress);
            return SdkResult<List<string>>.Fail("CERTIFICATION_ENTRIES_EXCEPTION", "获取未认证词条列表时发生异常");
        }
    }
}
