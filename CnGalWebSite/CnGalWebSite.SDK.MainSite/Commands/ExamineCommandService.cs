using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class ExamineCommandService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<ExamineCommandService> logger,
    ExamineUserCache userCache,
    ICurrentUserAccessor currentUserAccessor)
    : CommandServiceBase(httpClient, publicCache), IExamineCommandService
{

    protected override ILogger Logger => logger;

    public async Task<SdkResult<bool>> ProcExamineAsync(long id, bool isPassed, string comments = "", CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string? userId = null;
        var writeAttempted = false;
        ExamineProcResultModel? result = null;
        try
        {
            userId = await currentUserAccessor.GetCurrentUserIdAsync(cancellationToken);
            writeAttempted = true;
            result = await PostAsJsonAsync<ExamineProcModel, ExamineProcResultModel>(
                "api/examines/proc",
                new ExamineProcModel { Id = id, IsPassed = isPassed, Comments = comments },
                cancellationToken);

            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail("EXAMINE_PROC_FAILED", result?.Error ?? "审核处理失败");
            }

            return SdkResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "审核处理异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<bool>(ex, "EXAMINE_PROC_EXCEPTION", "审核处理时发生异常");
        }
        finally
        {
            if (writeAttempted)
            {
                userCache.InvalidateExamine(HttpClient.BaseAddress, id);
                userCache.InvalidateReviewQueue(HttpClient.BaseAddress);
                userCache.Invalidate(HttpClient.BaseAddress, userId);
                if (result is not null)
                {
                    userCache.Invalidate(HttpClient.BaseAddress, result.ApplicationUserId);
                }
            }
        }
    }

    public async Task<SdkResult<bool>> EditReviewStateAsync(long[] examineIds, EditRecordReviewState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string? userId = null;
        var writeAttempted = false;
        try
        {
            userId = await currentUserAccessor.GetCurrentUserIdAsync(cancellationToken);
            writeAttempted = true;
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "审阅状态修改异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<bool>(ex, "EXAMINE_REVIEW_STATE_EXCEPTION", "审阅状态修改时发生异常");
        }
        finally
        {
            if (writeAttempted)
            {
                userCache.Invalidate(HttpClient.BaseAddress, userId);
            }
        }
    }

    public async Task<SdkResult<bool>> EditUserMonitorsAsync(int[] ids, bool inMonitor, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string? userId = null;
        var writeAttempted = false;
        try
        {
            userId = await currentUserAccessor.GetCurrentUserIdAsync(cancellationToken);
            writeAttempted = true;
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "监控设置修改异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<bool>(ex, "EXAMINE_MONITOR_EXCEPTION", "监控设置修改时发生异常");
        }
        finally
        {
            if (writeAttempted)
            {
                userCache.Invalidate(HttpClient.BaseAddress, userId);
            }
        }
    }

}
