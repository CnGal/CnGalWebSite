using CnGalWebSite.Core.Models;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.TimedTask.Models.ViewModels;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

/// <summary>
/// 定时任务查询与命令服务接口（TimedTask 外部 API）。
/// </summary>
public interface ITimedTaskService
{
    /// <summary>
    /// 分页查询定时任务列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<TimedTaskOverviewModel>>> GetTimedTasksAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 运行指定定时任务。
    /// </summary>
    Task<SdkResult<bool>> RunTimedTaskAsync(int[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// 暂停/恢复定时任务。
    /// </summary>
    Task<SdkResult<bool>> PauseTimedTaskAsync(int[] ids, bool isPause, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取定时任务编辑模型。
    /// </summary>
    Task<SdkResult<TimedTaskEditModel>> GetTimedTaskEditAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 编辑或添加定时任务。
    /// </summary>
    Task<SdkResult<bool>> EditTimedTaskAsync(TimedTaskEditModel model, CancellationToken cancellationToken = default);
}
