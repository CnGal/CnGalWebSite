namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 全局 Toast 通知服务实现
/// </summary>
public class CgToastService : ICgToastService
{
    public event Action<string, string?, string>? OnShow;

    public void Show(string title, string? detail = null, string variant = "info")
    {
        OnShow?.Invoke(title, detail, variant);
    }
}
