namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 全局 Toast 通知服务接口
/// </summary>
public interface ICgToastService
{
    /// <summary>
    /// 当需要显示 Toast 时触发。参数：(message, variant)
    /// </summary>
    event Action<string, string>? OnShow;

    /// <summary>
    /// 显示一条 Toast 消息
    /// </summary>
    /// <param name="message">提示文字</param>
    /// <param name="variant">类型：info / success / warning / error</param>
    void Show(string message, string variant = "info");
}
