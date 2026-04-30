using System;
using System.Threading.Tasks;

namespace CnGalWebSite.MainSite.Shared.Services;

public interface ICgThemeService
{
    event Action? ThemeChanged;

    bool IsDarkMode { get; }

    Task SetThemeModeAsync(bool isDark);

    void OnThemeChanged();

    Task CheckAsync();
}
