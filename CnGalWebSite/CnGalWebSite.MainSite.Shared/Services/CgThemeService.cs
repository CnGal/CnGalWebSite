using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

public class CgThemeService : ICgThemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;
    private bool _isDarkMode;

    public event Action? ThemeChanged;

    public bool IsDarkMode
    {
        get => _isDarkMode;
    }

    public CgThemeService(
        IHttpContextAccessor httpContextAccessor,
        IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
        _isDarkMode = ResolveInitialTheme();
        PersistThemeCookieOnServer(_isDarkMode);
    }

    public async Task SetThemeModeAsync(bool isDark)
    {
        var needNotify = _isDarkMode != isDark;
        _isDarkMode = isDark;

        PersistThemeCookieOnServer(isDark);
        await PersistThemeOnClientAsync(isDark);

        if (needNotify)
        {
            OnThemeChanged();
        }
    }

    public async Task CheckAsync()
    {
        var needNotify = false;
        try
        {
            var cookieValue = await _jsRuntime.InvokeAsync<string>("cngalTheme.getCookie");
            if (!string.IsNullOrEmpty(cookieValue))
            {
                var isDarkFromCookie = string.Equals(cookieValue, ThemeConstants.DarkValue, StringComparison.OrdinalIgnoreCase);
                if (_isDarkMode != isDarkFromCookie)
                {
                    _isDarkMode = isDarkFromCookie;
                    needNotify = true;
                }
            }
            else
            {
                var legacyValue = await _jsRuntime.InvokeAsync<string>("cngalTheme.getLegacy");
                if (!string.IsNullOrEmpty(legacyValue))
                {
                    var isDarkFromLegacy = string.Equals(legacyValue, ThemeConstants.DarkValue, StringComparison.OrdinalIgnoreCase);
                    if (_isDarkMode != isDarkFromLegacy)
                    {
                        _isDarkMode = isDarkFromLegacy;
                        needNotify = true;
                    }
                }
            }

            await PersistThemeOnClientAsync(_isDarkMode);
        }
        catch
        {
            // SSR without JS interop, ignore
        }

        if (needNotify)
        {
            OnThemeChanged();
        }
    }

    public void OnThemeChanged()
    {
        ThemeChanged?.Invoke();
    }

    private bool ResolveInitialTheme()
    {
        if (TryGetThemeFromCookie(out var isDarkMode))
        {
            return isDarkMode;
        }

        return false;
    }

    private bool TryGetThemeFromCookie(out bool isDarkMode)
    {
        var cookieValue = _httpContextAccessor.HttpContext?.Request.Cookies[ThemeConstants.CookieName];
        isDarkMode = string.Equals(cookieValue, ThemeConstants.DarkValue, StringComparison.OrdinalIgnoreCase);
        return cookieValue is not null;
    }

    private void PersistThemeCookieOnServer(bool isDark)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.Cookies.Append(
            ThemeConstants.CookieName,
            isDark ? ThemeConstants.DarkValue : ThemeConstants.LightValue,
            new CookieOptions
            {
                Path = "/",
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = httpContext.Request.IsHttps,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
    }

    private async Task PersistThemeOnClientAsync(bool isDark)
    {
        await _jsRuntime.InvokeVoidAsync("cngalTheme.set", isDark);
    }
}
