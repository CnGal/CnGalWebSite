using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

public sealed class AccessTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var accessToken = httpContext is null ? null : await httpContext.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken) is false)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
