using CnGalWebSite.DataModel.Model;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IExamineCommandService
{
    /// <summary>
    /// 通过或驳回审核。
    /// </summary>
    Task<SdkResult<bool>> ProcExamineAsync(long id, bool isPassed, string comments = "", CancellationToken cancellationToken = default);

    /// <summary>
    /// 修改审阅状态（已读/忽略）。
    /// </summary>
    Task<SdkResult<bool>> EditReviewStateAsync(long[] examineIds, EditRecordReviewState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加或取消关注词条。
    /// </summary>
    Task<SdkResult<bool>> EditUserMonitorsAsync(int[] ids, bool inMonitor, CancellationToken cancellationToken = default);
}
