using CnGalWebSite.Core.Models;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.TimedTask.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class TimedTaskService(HttpClient httpClient, ILogger<TimedTaskService> logger)
    : CommandServiceBase(httpClient), ITimedTaskService
{
    protected override ILogger Logger => logger;

    // ─── 查询 ───

    public async Task<SdkResult<QueryResultModel<TimedTaskOverviewModel>>> GetTimedTasksAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await PostAsJsonRawAsync(
                "api/timedtasks/List", parameter, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "定时任务列表接口请求失败。StatusCode={StatusCode}; BaseAddress={BaseAddress}",
                    (int)response.StatusCode, HttpClient.BaseAddress);
                return SdkResult<QueryResultModel<TimedTaskOverviewModel>>.Fail(
                    "TIMED_TASKS_HTTP_FAILED",
                    $"获取定时任务列表失败（HTTP {(int)response.StatusCode}）");
            }

            var result = await ReadResponseAsync<QueryResultModel<TimedTaskOverviewModel>>(response, cancellationToken);
            return SdkResult<QueryResultModel<TimedTaskOverviewModel>>.Ok(result!);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取定时任务列表异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<TimedTaskOverviewModel>>.Fail(
                "TIMED_TASKS_EXCEPTION",
                "请求定时任务列表时发生异常");
        }
    }

    // ─── 命令 ───

    public async Task<SdkResult<bool>> RunTimedTaskAsync(int[] ids, CancellationToken cancellationToken = default)
    {
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
        catch (Exception ex)
        {
            Logger.LogError(ex, "运行定时任务异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("RUN_TIMED_TASK_EXCEPTION", "运行定时任务时发生异常");
        }
    }

    public async Task<SdkResult<bool>> PauseTimedTaskAsync(int[] ids, bool isPause, CancellationToken cancellationToken = default)
    {
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
        catch (Exception ex)
        {
            Logger.LogError(ex, "操作定时任务异常。BaseAddress={BaseAddress}", HttpClient.BaseAddress);
            return SdkResult<bool>.Fail("PAUSE_TIMED_TASK_EXCEPTION", "操作定时任务时发生异常");
        }
    }
}
