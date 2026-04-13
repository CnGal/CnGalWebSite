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
            var result = await PostAsJsonAsync<ExamineProcModel, Result>(
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
}
