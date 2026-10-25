using System.Text.RegularExpressions;
using CnGalWebSite.MainSite.Shared.Services;

namespace CnGalWebSite.MainSite.Services;

internal static partial class PublicHtmlCachePolicy
{
    // Only cookies that can change the anonymous HTML variant block shared caching.
    // Identity is checked separately through User/Authorization headers.
    private static readonly HashSet<string> OutputVariantCookies = new(StringComparer.OrdinalIgnoreCase)
    {
        ThemeConstants.CookieName,
        MiniModeConstants.CookieName
    };

    public static bool CanShare(HttpContext context)
    {
        if (context.RequestServices.GetRequiredService<PageResponseState>().Ready != true
            || !HttpMethods.IsGet(context.Request.Method) || context.Response.StatusCode != 200
            || context.User.Identity?.IsAuthenticated == true
            || context.Request.Headers.ContainsKey("Authorization")
            || context.Response.Headers.ContainsKey("Set-Cookie")
            || context.Request.Cookies.Keys.Any(key => !OutputVariantCookies.Contains(key))
            || context.Request.Query.ContainsKey(MiniModeConstants.QueryParameterName)) return false;

        var path = context.Request.Path.Value ?? "";
        return path is "/" or "/search" || DetailRoute().IsMatch(path);
    }

    [GeneratedRegex(@"^/(entries|articles|videos|tags|peripheries)/index/[0-9]+/?$", RegexOptions.IgnoreCase)]
    private static partial Regex DetailRoute();
}
