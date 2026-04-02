using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class AdminCommandService(HttpClient httpClient, ILogger<AdminCommandService> logger)
    : CommandServiceBase(httpClient), IAdminCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<bool>> RefreshSearchDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>("api/admin/RefreshSearchData", cancellationToken);
            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("ADMIN_REFRESH_SEARCH_FAILED", result?.Error ?? "刷新搜索缓存失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "刷新搜索缓存异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("ADMIN_REFRESH_SEARCH_EXCEPTION", "刷新搜索缓存时发生异常");
        }
    }

    public async Task<SdkResult<bool>> RunTempFunctionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>("api/admin/TempFunction", cancellationToken);
            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("ADMIN_TEMP_FUNCTION_FAILED", result?.Error ?? "执行临时脚本失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "执行临时脚本异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("ADMIN_TEMP_FUNCTION_EXCEPTION", "执行临时脚本时发生异常");
        }
    }

    public async Task<SdkResult<bool>> EditCommentPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<EditCommentPriorityViewModel, Result>(
                "api/comments/EditCommentPriority",
                new EditCommentPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
                cancellationToken);
            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("ADMIN_EDIT_PRIORITY_FAILED", result?.Error ?? "调整评论优先级失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "调整评论优先级异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("ADMIN_EDIT_PRIORITY_EXCEPTION", "调整评论优先级时发生异常");
        }
    }

    public async Task<SdkResult<bool>> HideCommentAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<HiddenCommentModel, Result>(
                "api/comments/HiddenComment",
                new HiddenCommentModel { Ids = ids, IsHidden = isHidden },
                cancellationToken);
            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("ADMIN_HIDE_COMMENT_FAILED", result?.Error ?? "操作评论失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "操作评论异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("ADMIN_HIDE_COMMENT_EXCEPTION", "操作评论时发生异常");
        }
    }
}
