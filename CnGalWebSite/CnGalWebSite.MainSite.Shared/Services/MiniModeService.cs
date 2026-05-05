using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 迷你模式（审核模式）状态服务
/// </summary>
public class MiniModeService : IMiniModeService
{
    private readonly NavigationManager _navigationManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private bool _isMiniMode;

    public event Action? MiniModeChanged;

    public bool IsMiniMode
    {
        get => _isMiniMode;
    }

    public MiniModeService(
        NavigationManager navigationManager,
        IHttpContextAccessor httpContextAccessor,
        IJSRuntime jsRuntime,
        ILocalStorageService localStorageService)
    {
        _navigationManager = navigationManager;
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
        _localStorage = localStorageService;
        _isMiniMode = ResolveInitialMiniMode(_navigationManager.Uri);
        PersistMiniModeCookieOnServer(_isMiniMode);
    }

    public async Task SetMiniModeAsync(bool isMiniMode)
    {
        var needRefresh = _isMiniMode != isMiniMode;
        _isMiniMode = isMiniMode;

        PersistMiniModeCookieOnServer(isMiniMode);
        await PersistMiniModeOnClientAsync(isMiniMode);

        if (needRefresh)
        {
            OnMiniModeChanged();
        }
    }

    public async Task CheckAsync()
    {
        var needRefresh = false;
        try
        {
            // 只要当前地址带 ref=gov，就立即进入迷你模式并持久化。
            if (HasMiniModeRef(_navigationManager.Uri))
            {
                if (!_isMiniMode)
                {
                    _isMiniMode = true;
                    needRefresh = true;
                }
            }
            else
            {
                var isMiniModeInCookie = await _jsRuntime.InvokeAsync<bool>("cngalMiniMode.getCookie");
                if (isMiniModeInCookie)
                {
                    if (!_isMiniMode)
                    {
                        _isMiniMode = true;
                        needRefresh = true;
                    }
                }
                else
                {
                    var isMiniModeInLegacyStorage = await _localStorage.GetItemAsync<bool>("IsMiniMode");
                    if (isMiniModeInLegacyStorage && !_isMiniMode)
                    {
                        _isMiniMode = true;
                        needRefresh = true;
                    }
                }
            }

            await PersistMiniModeOnClientAsync(_isMiniMode);
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

    private bool ResolveInitialMiniMode(string uri)
    {
        if (HasMiniModeRef(uri))
        {
            return true;
        }

        if (TryGetMiniModeFromCookie(out var isMiniMode))
        {
            return isMiniMode;
        }

        return false;
    }

    private bool TryGetMiniModeFromCookie(out bool isMiniMode)
    {
        var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[MiniModeConstants.CookieName];
        isMiniMode = string.Equals(cookieValue, MiniModeConstants.CookieValue, StringComparison.OrdinalIgnoreCase);
        return cookieValue is not null;
    }

    private static bool HasMiniModeRef(string uri)
    {
        var query = QueryHelpers.ParseQuery(new Uri(uri).Query);
        if (!query.TryGetValue(MiniModeConstants.QueryParameterName, out var values))
        {
            return false;
        }

        foreach (var value in values)
        {
            if (string.Equals(value, MiniModeConstants.QueryParameterValue, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void PersistMiniModeCookieOnServer(bool isMiniMode)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.Response.HasStarted)
        {
            return;
        }

        if (isMiniMode)
        {
            httpContext.Response.Cookies.Append(
                MiniModeConstants.CookieName,
                MiniModeConstants.CookieValue,
                CreateCookieOptions(httpContext));
            return;
        }

        httpContext.Response.Cookies.Delete(
            MiniModeConstants.CookieName,
            new CookieOptions
            {
                Path = "/"
            });
    }

    private async Task PersistMiniModeOnClientAsync(bool isMiniMode)
    {
        await _localStorage.SetItemAsync("IsMiniMode", isMiniMode);
        await _jsRuntime.InvokeVoidAsync("cngalMiniMode.setCookie", isMiniMode);
    }

    private static CookieOptions CreateCookieOptions(HttpContext httpContext)
    {
        return new CookieOptions
        {
            Path = "/",
            HttpOnly = false,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = httpContext.Request.IsHttps,
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        };
    }
}
