using CnGalWebSite.MainSite.Shared.Services;

namespace CnGalWebSite.MainSite.Services;

internal static class PublicHtmlCachePolicy
{
    public static bool CanShare(HttpContext context)
    {
        // Theme is client-side only, so cg-theme does not create a server HTML variant.
        // MiniMode changes the SSR output and must never enter a shared cache.
        return context.RequestServices.GetRequiredService<PageResponseState>().Ready == true
            && HttpMethods.IsGet(context.Request.Method)
            && context.Response.StatusCode == 200
            && context.User.Identity?.IsAuthenticated != true
            && !context.Request.Headers.ContainsKey("Authorization")
            && !context.Response.Headers.ContainsKey("Set-Cookie")
            && !context.Request.Cookies.ContainsKey(MiniModeConstants.CookieName)
            && !context.Request.Query.ContainsKey(MiniModeConstants.QueryParameterName)
            && !context.Request.Cookies.Keys.Any(key => !string.Equals(key, ThemeConstants.CookieName, StringComparison.OrdinalIgnoreCase));
    }
}
