using CnGalWebSite.MainSite.Shared.Services.KanbanModels;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 看板娘事件循环服务接口 — 使用 PeriodicTimer 驱动后台事件巡检
/// </summary>
public interface IKanbanEventService
{
    /// <summary>
    /// 当事件服务决定弹出对话框时触发
    /// </summary>
    event Action<KanbanDialogBoxModel>? OnDialogTriggered;

    /// <summary>
    /// 启动事件循环。调用后会阻塞直到 ct 被取消。
    /// 应由 Blazor 组件在后台调用，传入组件管理的 CancellationToken。
    /// </summary>
    Task StartAsync(CancellationToken ct);

    /// <summary>
    /// 通知事件服务用户发生了交互（重置空闲计时器）
    /// </summary>
    void NotifyUserInteraction();

    /// <summary>
    /// 从 JS 层触发自定义事件（由 window.Live2dHitHeadEvent / Live2dHitBodyEvent 回调）
    /// </summary>
    Task TriggerCustomEventAsync(string name);

    /// <summary>
    /// 持久化当前事件触发时间到 LocalStorage
    /// </summary>
    Task SaveAsync();
}
