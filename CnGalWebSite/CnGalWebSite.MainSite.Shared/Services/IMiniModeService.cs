using System;
using System.Threading.Tasks;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 迷你模式（审核模式）状态服务接口
/// </summary>
public interface IMiniModeService
{
    /// <summary>
    /// 迷你模式发生变化事件
    /// </summary>
    event Action? MiniModeChanged;

    /// <summary>
    /// 是否为迷你模式
    /// </summary>
    bool IsMiniMode { get; }

    /// <summary>
    /// 设置迷你模式并同步持久化状态
    /// </summary>
    Task SetMiniModeAsync(bool isMiniMode);

    /// <summary>
    /// 触发状态变化事件
    /// </summary>
    void OnMiniModeChanged();

    /// <summary>
    /// 检查并同步客户端持久化状态
    /// </summary>
    Task CheckAsync();
}
