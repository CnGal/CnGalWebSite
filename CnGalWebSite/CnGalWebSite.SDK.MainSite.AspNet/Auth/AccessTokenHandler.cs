using System.Net;
using System.Net.Http.Headers;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

/// <summary>由应用作用域创建；InnerHandler 为工厂维护的共享传输管线。</summary>
public sealed class AccessTokenHandler(CircuitTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var snapshot = await tokenStore.GetSnapshotAsync(cancellationToken);
        if (snapshot is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", snapshot.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || snapshot is null) return response;

        AuthenticationSnapshot? refreshed;
        try { refreshed = await tokenStore.RefreshAsync(snapshot, cancellationToken); }
        catch { response.Dispose(); throw; }
        if (refreshed is null || refreshed.AccessToken == snapshot.AccessToken
            || refreshed.ExpiresAt <= DateTimeOffset.UtcNow || refreshed.CacheScope.IdentityChanged.IsCancellationRequested) return response;

        // 查询绑定的旧身份会在刷新时失效，由查询层用新身份至多重试一次。
        // 其他 SDK 调用保留现有的 401 刷新后重试行为。
        try
        {
            using var retry = await CloneHttpRequestMessageAsync(request, cancellationToken);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
            return await base.SendAsync(retry, cancellationToken);
        }
        finally { response.Dispose(); }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };
        try
        {
            if (request.Content is not null)
            {
                clone.Content = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(cancellationToken));
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            foreach (var header in request.Headers) clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            foreach (var (key, value) in request.Options) clone.Options.TryAdd(key, value);
            return clone;
        }
        catch { clone.Dispose(); throw; }
    }
}
