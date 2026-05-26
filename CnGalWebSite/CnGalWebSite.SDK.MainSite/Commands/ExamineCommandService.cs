using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class ExamineCommandService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<ExamineCommandService> logger)
    : CommandServiceBase(httpClient), IExamineCommandService
{
    protected override ILogger Logger => logger;

    public async Task<SdkResult<bool>> ProcExamineAsync(long id, bool isPassed, string comments = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<ExamineProcModel, ExamineProcResultModel>(
                "api/examines/proc",
                new ExamineProcModel { Id = id, IsPassed = isPassed, Comments = comments },
                cancellationToken);

            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("EXAMINE_PROC_FAILED", result?.Error ?? "审核处理失败");
            }

            memoryCache.Remove("main-site:user-content-center");
            memoryCache.Remove($"main-site:examine-detail:{id}");
            memoryCache.Remove($"main-site:examine-overview:{id}");
            InvalidateEntityCaches(result);
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "审核处理异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("EXAMINE_PROC_EXCEPTION", "审核处理时发生异常");
        }
    }

    public async Task<SdkResult<bool>> EditReviewStateAsync(long[] examineIds, EditRecordReviewState state, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<EditUserReviewEditRecordStateModel, Result>(
                "api/examines/EditUserReviewEditRecordState",
                new EditUserReviewEditRecordStateModel { ExamineIds = examineIds, State = state },
                cancellationToken);

            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("EXAMINE_REVIEW_STATE_FAILED", result?.Error ?? "审阅状态修改失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "审阅状态修改异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("EXAMINE_REVIEW_STATE_EXCEPTION", "审阅状态修改时发生异常");
        }
    }

    public async Task<SdkResult<bool>> EditUserMonitorsAsync(int[] ids, bool inMonitor, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await PostAsJsonAsync<EditUserMonitorsModel, Result>(
                "api/examines/EditUserMonitors",
                new EditUserMonitorsModel { Ids = ids, InMonitor = inMonitor },
                cancellationToken);

            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("EXAMINE_MONITOR_FAILED", result?.Error ?? "监控设置修改失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "监控设置修改异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("EXAMINE_MONITOR_EXCEPTION", "监控设置修改时发生异常");
        }
    }

    private void InvalidateEntityCaches(ExamineProcResultModel result)
    {
        switch (result.Operation)
        {
            case Operation.EstablishMain:
            case Operation.EstablishAddInfor:
            case Operation.EstablishMainPage:
            case Operation.EstablishImages:
            case Operation.EstablishRelevances:
            case Operation.EstablishTags:
            case Operation.EstablishAudio:
            case Operation.EstablishWebsite:
                if (result.EntryId.HasValue)
                {
                    memoryCache.Remove($"main-site:entry-detail:{result.EntryId.Value}");
                    memoryCache.Remove($"main-site:entry-edit-records:{result.EntryId.Value}");
                }
                memoryCache.Remove("main-site:home-summary");
                break;

            case Operation.EditArticleMain:
            case Operation.EditArticleRelevanes:
            case Operation.EditArticleMainPage:
                if (result.ArticleId.HasValue)
                {
                    memoryCache.Remove($"main-site:article-detail:{result.ArticleId.Value}");
                    memoryCache.Remove($"main-site:article-edit-records:{result.ArticleId.Value}");
                }
                memoryCache.Remove("main-site:home-summary");
                break;

            case Operation.EditTagMain:
            case Operation.EditTagChildTags:
            case Operation.EditTagChildEntries:
                if (result.TagId.HasValue)
                {
                    memoryCache.Remove($"main-site:tag-detail:{result.TagId.Value}");
                    memoryCache.Remove($"main-site:tag-edit-records:{result.TagId.Value}");
                    memoryCache.Remove("main-site:tag-tree");
                }
                break;

            case Operation.EditVideoMain:
            case Operation.EditVideoImages:
            case Operation.EditVideoMainPage:
            case Operation.EditVideoRelevanes:
                if (result.VideoId.HasValue)
                {
                    memoryCache.Remove($"main-site:video-detail:{result.VideoId.Value}");
                    memoryCache.Remove($"main-site:video-edit-records:{result.VideoId.Value}");
                }
                memoryCache.Remove("main-site:home-summary");
                break;

            case Operation.EditPeripheryMain:
            case Operation.EditPeripheryImages:
            case Operation.EditPeripheryRelatedEntries:
            case Operation.EditPeripheryRelatedPeripheries:
                if (result.PeripheryId.HasValue)
                {
                    memoryCache.Remove($"main-site:periphery-detail:{result.PeripheryId.Value}");
                    memoryCache.Remove($"main-site:periphery-edit-records:{result.PeripheryId.Value}");
                }
                break;

            case Operation.UserMainPage:
            case Operation.EditUserMain:
                if (!string.IsNullOrEmpty(result.ApplicationUserId))
                {
                    memoryCache.Remove($"main-site:space-detail:{result.ApplicationUserId}");
                }
                break;

            case Operation.EditPlayedGameMain:
                if (result.PlayedGameId.HasValue)
                {
                    memoryCache.Remove($"main-site:played-game-overview:{result.PlayedGameId.Value}");
                }
                break;
        }
    }
}
