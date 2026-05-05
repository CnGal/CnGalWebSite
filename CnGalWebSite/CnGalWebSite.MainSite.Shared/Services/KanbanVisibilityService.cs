using Blazored.LocalStorage;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 看板娘 / 浮动工具栏 可见性切换服务实现
/// 状态持久化到 LocalStorage，key: kanban_live2d_show_kanban
/// </summary>
public class KanbanVisibilityService : IKanbanVisibilityService
{
    private const string StorageKey = "kanban_live2d_show_kanban";

    private readonly ILocalStorageService _localStorage;
    private bool _showKanban = true; // 默认显示看板娘
    private bool _loaded;

    public event Action? OnVisibilityChanged;

    public bool ShowKanban => _showKanban;

    public KanbanVisibilityService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task LoadAsync()
    {
        if (_loaded)
        {
            return;
        }

        try
        {
            if (await _localStorage.ContainKeyAsync(StorageKey))
            {
                _showKanban = await _localStorage.GetItemAsync<bool>(StorageKey);
            }
        }
        catch
        {
            // localStorage 不可用时保持默认值
            _showKanban = true;
        }

        _loaded = true;
    }

    public async Task ToggleAsync()
    {
        await SetShowKanbanAsync(!_showKanban);
    }

    public async Task SetShowKanbanAsync(bool showKanban)
    {
        if (_showKanban == showKanban)
        {
            return;
        }

        _showKanban = showKanban;

        try
        {
            await _localStorage.SetItemAsync(StorageKey, _showKanban);
        }
        catch
        {
            // localStorage 写入失败时忽略，内存状态仍然有效
        }

        OnVisibilityChanged?.Invoke();
    }
}
