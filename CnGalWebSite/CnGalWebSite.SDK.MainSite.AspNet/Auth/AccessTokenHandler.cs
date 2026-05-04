using System.Net;
using System.Net.Http.Headers;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

public sealed class AccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CookieOidcRefresher _cookieOidcRefresher;
    private readonly IOptions<MainSiteOidcOptions> _oidcOptions;

    public AccessTokenHandler(
        IHttpContextAccessor httpContextAccessor,
        CookieOidcRefresher cookieOidcRefresher,
        IOptions<MainSiteOidcOptions> oidcOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieOidcRefresher = cookieOidcRefresher;
        _oidcOptions = oidcOptions;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var tokenStore = httpContext?.RequestServices.GetService<CircuitTokenStore>();
        var refreshLeadTime = TimeSpan.FromMinutes(Math.Max(1, _oidcOptions.Value.RefreshLeadTimeMinutes));
        var oidcScheme = MainSiteOidcOptions.OidcScheme;

        await EnsureTokenAsync(httpContext, tokenStore, refreshLeadTime, oidcScheme, cancellationToken);

        var accessToken = tokenStore?.AccessToken
            ?? (httpContext is not null ? await httpContext.GetTokenAsync("access_token") : null);

        if (string.IsNullOrWhiteSpace(accessToken) is false)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized || httpContext is null)
        {
            return response;
        }

        var refreshResult = await _cookieOidcRefresher.TryRefreshAsync(httpContext, oidcScheme);
        if (refreshResult is not null)
        {
            var newExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(refreshResult.Value.ExpiresIn);
            tokenStore?.StoreTokens(refreshResult.Value.AccessToken, refreshResult.Value.RefreshToken, newExpiresAt);

            var retryRequest = await CloneHttpRequestMessageAsync(request, cancellationToken);
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResult.Value.AccessToken);

            response.Dispose();
            response = await base.SendAsync(retryRequest, cancellationToken);
        }
        else
        {
            tokenStore?.Clear();
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        return response;
    }

    private async Task EnsureTokenAsync(
        HttpContext? httpContext,
        CircuitTokenStore? tokenStore,
        TimeSpan refreshLeadTime,
        string oidcScheme,
        CancellationToken cancellationToken)
    {
        if (tokenStore?.AccessToken is not null)
        {
            if (tokenStore.NeedsRefresh(refreshLeadTime) && httpContext is not null)
            {
                var refreshResult = await _cookieOidcRefresher.TryRefreshAsync(httpContext, oidcScheme);
                if (refreshResult is not null)
                {
                    var newExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(refreshResult.Value.ExpiresIn);
                    tokenStore.StoreTokens(refreshResult.Value.AccessToken, refreshResult.Value.RefreshToken, newExpiresAt);
                }
            }
            return;
        }

        if (httpContext is null || tokenStore is null)
        {
            return;
        }

        var accessToken = await httpContext.GetTokenAsync("access_token");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return;
        }

        var refreshToken = await httpContext.GetTokenAsync("refresh_token");
        var expiresAtText = await httpContext.GetTokenAsync("expires_at");
        DateTimeOffset.TryParse(expiresAtText, out var expiresAt);
        tokenStore.StoreTokens(accessToken, refreshToken ?? string.Empty, expiresAt);

        if (tokenStore.NeedsRefresh(refreshLeadTime))
        {
            var refreshResult = await _cookieOidcRefresher.TryRefreshAsync(httpContext, oidcScheme);
            if (refreshResult is not null)
            {
                var newExpiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(refreshResult.Value.ExpiresIn);
                tokenStore.StoreTokens(refreshResult.Value.AccessToken, refreshResult.Value.RefreshToken, newExpiresAt);
            }
        }
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content is not null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(contentBytes);
            if (request.Content.Headers.ContentType is not null)
            {
                clone.Content.Headers.ContentType = request.Content.Headers.ContentType;
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var (key, value) in request.Options)
        {
            clone.Options.TryAdd(key, value);
        }

        clone.Version = request.Version;
        clone.VersionPolicy = request.VersionPolicy;

        return clone;
    }
}
