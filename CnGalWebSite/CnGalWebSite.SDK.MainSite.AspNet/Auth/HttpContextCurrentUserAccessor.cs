using CnGalWebSite.SDK.MainSite.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CnGalWebSite.SDK.MainSite.AspNet.Auth;

public sealed class HttpContextCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public string? GetCurrentUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
    }
}
