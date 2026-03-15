namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 全局 Toast 通知服务接口
/// </summary>
public interface ICgToastService
{
    /// <summary>
    /// 当需要显示 Toast 时触发。参数：(title, detail, variant)
    /// </summary>
    event Action<string, string?, string>? OnShow;

    /// <summary>
    /// 显示一条 Toast 消息
    /// </summary>
    /// <param name="title">标题文字</param>
    /// <param name="detail">详细描述（可选）</param>
    /// <param name="variant">类型：info / success / warning / error</param>
    void Show(string title, string? detail = null, string variant = "info");
}
