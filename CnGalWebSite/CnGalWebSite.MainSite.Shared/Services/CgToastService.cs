namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 全局 Toast 通知服务实现
/// </summary>
public class CgToastService : ICgToastService
{
    private readonly object _gate = new();
    private readonly Queue<CgToastMessage> _pending = new();
    public event Action<string, string?, string>? OnShow;

    public void Show(string title, string? detail = null, string variant = "info")
    {
        Action<string, string?, string>? handler;
        lock (_gate)
        {
            handler = OnShow;
            if (handler is null)
            {
                _pending.Enqueue(new(title, detail, variant));
                while (_pending.Count > 5) _pending.Dequeue();
            }
        }
        handler?.Invoke(title, detail, variant);
    }

    public IReadOnlyList<CgToastMessage> DrainPending()
    {
        lock (_gate) { var messages = _pending.ToArray(); _pending.Clear(); return messages; }
    }
}
