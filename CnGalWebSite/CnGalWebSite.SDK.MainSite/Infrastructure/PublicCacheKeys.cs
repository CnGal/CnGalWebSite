namespace CnGalWebSite.SDK.MainSite.Infrastructure;

internal static class PublicCacheKeys
{
    public const string HomeSummary = "main-site:home-summary";
    public static string Detail(string domain, long id) => $"main-site:{domain}-detail:{id}";
}
