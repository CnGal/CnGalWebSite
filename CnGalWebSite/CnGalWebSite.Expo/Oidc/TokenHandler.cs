using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace CnGalWebSite.Expo.Helpers;

public class TokenHandler(IHttpContextAccessor httpContextAccessor) :
    DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await httpContextAccessor.HttpContext?.GetTokenAsync("access_token");

        if (string.IsNullOrWhiteSpace(accessToken) == false)
        {
            request.Headers.Authorization =
                      new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
