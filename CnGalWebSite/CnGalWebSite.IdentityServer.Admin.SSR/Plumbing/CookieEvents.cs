using CnGalWebSite.Extensions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Plumbing;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserAccessTokenStore _store;
    private readonly IUserAccessTokenManagementService _userAccessTokenManagementService;

    public CookieEvents(IUserAccessTokenStore store, IUserAccessTokenManagementService userAccessTokenManagementService)
    {
        _store = store;
        _userAccessTokenManagementService = userAccessTokenManagementService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        await base.ValidatePrincipal(context);

        var id = context?.Principal?.Claims?.GetUserId();
        if (string.IsNullOrWhiteSpace(id))
        {
            context.RejectPrincipal();
            return;
        }

        var token = await _userAccessTokenManagementService.GetUserAccessTokenAsync(context.Principal);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.RejectPrincipal();
        }
    }
}
