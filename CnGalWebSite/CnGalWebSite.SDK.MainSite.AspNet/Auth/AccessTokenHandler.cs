using System.Net;
using System.Net.Http.Headers;
using CnGalWebSite.SDK.MainSite.Infrastructure;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

/// <summary>由应用作用域创建；InnerHandler 为工厂维护的共享传输管线。</summary>
public sealed class AccessTokenHandler(CircuitTokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var snapshot = await tokenStore.GetSnapshotAsync(cancellationToken);
        var identityVersion = snapshot?.IdentityVersion ?? tokenStore.IdentityVersion;
        if (identityVersion != tokenStore.IdentityVersion
            || (snapshot is null && tokenStore.IdentityState == CnGalWebSite.SDK.MainSite.Abstractions.CurrentIdentityState.Authenticated))
            throw new SdkIdentityChangedException();
        if (snapshot is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", snapshot.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);
        await ValidateResponseAsync(response, identityVersion, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized || snapshot is null
            || !SdkRequestPolicy.CanReplay(request)) return response;

        AuthenticationSnapshot? refreshed;
        try { refreshed = await tokenStore.RefreshAsync(snapshot, cancellationToken); }
        catch { response.Dispose(); throw; }
        if (refreshed is null || refreshed.AccessToken == snapshot.AccessToken
            || refreshed.ExpiresAt <= DateTimeOffset.UtcNow || refreshed.CacheScope.IdentityChanged.IsCancellationRequested) return response;

        // 只有显式可重放的查询可以刷新后重发；写操作保留 401，避免重复业务动作。
        try
        {
            using var retry = await CloneHttpRequestMessageAsync(request, cancellationToken);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
            var retriedResponse = await base.SendAsync(retry, cancellationToken);
            await ValidateResponseAsync(retriedResponse, identityVersion, cancellationToken);
            return retriedResponse;
        }
        finally { response.Dispose(); }
    }

    private async Task ValidateResponseAsync(HttpResponseMessage response, long identityVersion, CancellationToken token)
    {
        try
        {
            // These SDK clients already use buffered JSON responses. Validate after the body, not just headers.
            await response.Content.LoadIntoBufferAsync(token);
            if (identityVersion != tokenStore.IdentityVersion) throw new SdkIdentityChangedException();
        }
        catch { response.Dispose(); throw; }
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
