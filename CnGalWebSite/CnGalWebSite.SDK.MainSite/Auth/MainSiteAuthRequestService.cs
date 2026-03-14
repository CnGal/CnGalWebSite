using CnGalWebSite.SDK.MainSite.Abstractions;

namespace CnGalWebSite.SDK.MainSite.Auth;

public sealed class MainSiteAuthRequestService : IMainSiteAuthRequestService
{
    private const string AuthenticationPrefix = "/authentication";

    public string NormalizeReturnUrl(string? returnUrl)
    {
        const string defaultReturnUrl = "/";

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return defaultReturnUrl;
        }

        if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            return returnUrl.StartsWith('/') ? returnUrl : $"/{returnUrl}";
        }

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absoluteUri))
        {
            var pathAndQuery = absoluteUri.PathAndQuery;
            return string.IsNullOrWhiteSpace(pathAndQuery) ? defaultReturnUrl : pathAndQuery;
        }

        return defaultReturnUrl;
    }

    public string BuildLoginUrl(string? returnUrl)
    {
        var safeReturnUrl = Uri.EscapeDataString(NormalizeReturnUrl(returnUrl));
        return $"{AuthenticationPrefix}/login?returnUrl={safeReturnUrl}";
    }

    public string BuildLogoutUrl(string? returnUrl)
    {
        var safeReturnUrl = Uri.EscapeDataString(NormalizeReturnUrl(returnUrl));
        return $"{AuthenticationPrefix}/logout?returnUrl={safeReturnUrl}";
    }
}
