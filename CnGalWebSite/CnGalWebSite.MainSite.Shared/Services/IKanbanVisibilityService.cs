using System;
using System.Threading.Tasks;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 看板娘 / 浮动工具栏 可见性切换服务接口
/// 管理 Live2D 看板娘与右下角浮动工具栏的互斥显示状态
/// </summary>
public interface IKanbanVisibilityService
{
    /// <summary>
    /// 可见性发生变化事件
    /// </summary>
    event Action? OnVisibilityChanged;

    /// <summary>
    /// 当前是否显示看板娘（true = 看板娘 Live2D，false = 浮动按钮工具栏）
    /// </summary>
    bool ShowKanban { get; }

    /// <summary>
    /// 切换显示模式（看板娘 ↔ 浮动工具栏）
    /// </summary>
    Task ToggleAsync();

    /// <summary>
    /// 设置显示模式并持久化
    /// </summary>
    /// <param name="showKanban">true = 显示看板娘，false = 显示浮动工具栏</param>
    Task SetShowKanbanAsync(bool showKanban);

    /// <summary>
    /// 从持久化存储加载初始状态（仅在首次渲染时调用）
    /// </summary>
    Task LoadAsync();
}
