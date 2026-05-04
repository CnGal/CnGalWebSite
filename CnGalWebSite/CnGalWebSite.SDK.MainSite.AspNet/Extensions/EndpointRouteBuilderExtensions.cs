using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace CnGalWebSite.SDK.MainSite.AspNet.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapMainSiteAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/authentication");

        group.MapGet("/login", (IMainSiteAuthRequestService authRequestService, IOptions<MainSiteOidcOptions> oidcOptions, string? returnUrl) =>
        {
            var persistentDays = Math.Max(1, oidcOptions.Value.PersistentLoginDays);
            var properties = new AuthenticationProperties
            {
                RedirectUri = authRequestService.NormalizeReturnUrl(returnUrl),
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(persistentDays)
            };
            return TypedResults.Challenge(properties, [MainSiteOidcOptions.OidcScheme]);
        }).AllowAnonymous();

        group.MapPost("/logout", (IMainSiteAuthRequestService authRequestService, string? returnUrl) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = authRequestService.NormalizeReturnUrl(returnUrl)
            };
            return TypedResults.SignOut(properties,
                [CookieAuthenticationDefaults.AuthenticationScheme, MainSiteOidcOptions.OidcScheme]);
        }).AllowAnonymous().DisableAntiforgery();

        return group;
    }
}
