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
    private readonly List<KeyValuePair<DateTime,string>> _failedCount = new List<KeyValuePair<DateTime, string>>();

    public CookieEvents(IUserAccessTokenStore store, IUserAccessTokenManagementService userAccessTokenManagementService)
    {
        _store = store;
        _userAccessTokenManagementService = userAccessTokenManagementService;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = await _store.GetTokenAsync(context.Principal);
        if (token == null) context.RejectPrincipal();

        await base.ValidatePrincipal(context);
    }

}
