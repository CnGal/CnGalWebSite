using CnGalWebSite.Extensions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Plumbing;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserAccessTokenManagementService _tokenManagementService;

    public CookieEvents(IUserAccessTokenManagementService tokenManagementService)
    {
        _tokenManagementService = tokenManagementService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = await _tokenManagementService.GetUserAccessTokenAsync(context.Principal);
        if (token == null)
        {
            context.RejectPrincipal();
        }

        await base.ValidatePrincipal(context);
    }

}
