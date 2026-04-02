using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IAdminCommandService
{
    /// <summary>
    /// 刷新搜索缓存（重建 ES 索引）。
    /// </summary>
    Task<SdkResult<bool>> RefreshSearchDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行预置的临时脚本。
    /// </summary>
    Task<SdkResult<bool>> RunTempFunctionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 调整评论优先级。
    /// </summary>
    Task<SdkResult<bool>> EditCommentPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);

    /// <summary>
    /// 隐藏或显示评论。
    /// </summary>
    Task<SdkResult<bool>> HideCommentAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);
}
