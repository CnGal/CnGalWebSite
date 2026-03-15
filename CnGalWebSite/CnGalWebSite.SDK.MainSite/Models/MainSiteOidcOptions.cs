namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class MainSiteOidcOptions
{
    public const string SectionName = "Oidc";
    public const string OidcScheme = "cngal";

    public string Authority { get; init; } = "https://localhost:5001";

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;

    public string[] Scopes { get; init; } = ["openid", "profile", "role", "CnGalAPI", "offline_access"];

    public int PersistentLoginDays { get; init; } = 30;

    public int RefreshLeadTimeMinutes { get; init; } = 5;
}
