using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 迷你模式（审核模式）状态服务
/// </summary>
public class MiniModeService : IMiniModeService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _isMiniMode;

    public event Action? MiniModeChanged;

    public bool IsMiniMode
    {
        get => _isMiniMode;
        set
        {
            if (_isMiniMode != value)
            {
                _isMiniMode = value;
                OnMiniModeChanged();
            }
        }
    }

    public MiniModeService(NavigationManager navigationManager, IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;

        // 判断来源，如果包含 ref=gov，则默认开启迷你模式
        if (navigationManager.Uri.Contains("ref=gov", StringComparison.OrdinalIgnoreCase))
        {
            _isMiniMode = true;
        }
    }

    public async Task CheckAsync()
    {
        var needRefresh = false;
        try
        {
            var str = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "IsMiniMode");
            var isMiniModeInStorage = str == "true";

            if (isMiniModeInStorage)
            {
                if (!IsMiniMode)
                {
                    _isMiniMode = true;
                    needRefresh = true;
                }
            }
            else
            {
                if (IsMiniMode)
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "IsMiniMode", "true");
                }
            }
        }
        catch
        {
            // SSR 无 JS 互操作，忽略报错
        }

        if (needRefresh)
        {
            OnMiniModeChanged();
        }
    }

    public void OnMiniModeChanged()
    {
        MiniModeChanged?.Invoke();
    }
}
