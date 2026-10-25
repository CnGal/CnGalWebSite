using CnGalWebSite.TimedTask.Models.Sorting;
using CnGalWebSite.Core.Models;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.TimedTask.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class TimedTaskService(HttpClient httpClient, PublicDataCache publicCache, ILogger<TimedTaskService> logger)
    : CommandServiceBase(httpClient, publicCache), ITimedTaskService
{
    protected override ILogger Logger => logger;

    // ─── 查询 ───

    public async Task<SdkResult<QueryResultModel<TimedTaskOverviewModel, TimedTaskSortField>>> GetTimedTasksAsync(
        QueryParameterModel<TimedTaskSortField> parameter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var response = await PostAsJsonRawAsync(
                "api/timedtasks/List", parameter, cancellationToken, readOnly: true);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "定时任务列表接口请求失败。StatusCode={StatusCode}; BaseAddress={BaseAddress}",
                    (int)response.StatusCode, HttpClient.BaseAddress);
                return SdkResult<QueryResultModel<TimedTaskOverviewModel, TimedTaskSortField>>.Fail(
                    "TIMED_TASKS_HTTP_FAILED",
                    $"获取定时任务列表失败（HTTP {(int)response.StatusCode}）");
            }

            var result = await ReadResponseAsync<QueryResultModel<TimedTaskOverviewModel, TimedTaskSortField>>(response, cancellationToken);
            return SdkResult<QueryResultModel<TimedTaskOverviewModel, TimedTaskSortField>>.Ok(result!);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取定时任务列表异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<QueryResultModel<TimedTaskOverviewModel, TimedTaskSortField>>(ex,
                "TIMED_TASKS_EXCEPTION",
                "请求定时任务列表时发生异常");
        }
    }

    // ─── 命令 ───

    public async Task<SdkResult<bool>> RunTimedTaskAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var result = await PostAsJsonAsync<RunTimedTaskModel, Result>(
                "api/timedtasks/RunTimedTask",
                new RunTimedTaskModel { Ids = ids },
                cancellationToken);

            if (result is null || !result.Success)
            {
                return SdkResult<bool>.Fail("RUN_TIMED_TASK_FAILED", result?.Message ?? "运行定时任务失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "运行定时任务异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<bool>(ex, "RUN_TIMED_TASK_EXCEPTION", "运行定时任务时发生异常");
        }
    }

    public async Task<SdkResult<bool>> PauseTimedTaskAsync(int[] ids, bool isPause, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var result = await PostAsJsonAsync<PauseTimedTaskModel, Result>(
                "api/timedtasks/PauseTimedTask",
                new PauseTimedTaskModel { Ids = ids, IsPause = isPause },
                cancellationToken);

            if (result is null || !result.Success)
            {
                return SdkResult<bool>.Fail("PAUSE_TIMED_TASK_FAILED", result?.Message ?? "操作定时任务失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "操作定时任务异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<bool>(ex, "PAUSE_TIMED_TASK_EXCEPTION", "操作定时任务时发生异常");
        }
    }

    // ─── 编辑 ───

    public async Task<SdkResult<TimedTaskEditModel>> GetTimedTaskEditAsync(long id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var result = await GetFromJsonAsync<TimedTaskEditModel>($"api/timedtasks/EditAsync?id={id}", cancellationToken);
            if (result is null)
            {
                return SdkResult<TimedTaskEditModel>.Fail("GET_TIMED_TASK_EDIT_FAILED", "获取定时任务详情失败");
            }
            return SdkResult<TimedTaskEditModel>.Ok(result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取定时任务详情异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<TimedTaskEditModel>(ex, "GET_TIMED_TASK_EDIT_EXCEPTION", "获取定时任务详情时发生异常");
        }
    }

    public async Task<SdkResult<bool>> EditTimedTaskAsync(TimedTaskEditModel model, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var result = await PostAsJsonAsync<TimedTaskEditModel, Result>(
                "api/timedtasks/EditAsync", model, cancellationToken);

            if (result is null || !result.Success)
            {
                return SdkResult<bool>.Fail("EDIT_TIMED_TASK_FAILED", result?.Message ?? "编辑定时任务失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "编辑定时任务异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkFailure.FromException<bool>(ex, "EDIT_TIMED_TASK_EXCEPTION", "编辑定时任务时发生异常");
        }
    }
}
