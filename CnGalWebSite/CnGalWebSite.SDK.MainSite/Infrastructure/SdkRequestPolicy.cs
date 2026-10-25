namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public static class SdkRequestPolicy
{
    public static readonly HttpRequestOptionsKey<bool> AllowReplay = new("CnGal.AllowReplay");

    public static bool CanReplay(HttpRequestMessage? request) => request is not null
        && request.Options.TryGetValue(AllowReplay, out var allowed) && allowed;

    public static HttpRequestMessage Read(HttpMethod method, string path, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, path) { Content = content };
        request.Options.Set(AllowReplay, true);
        return request;
    }
}
