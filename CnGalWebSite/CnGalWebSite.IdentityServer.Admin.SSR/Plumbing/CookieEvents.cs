﻿using CnGalWebSite.Extensions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Plumbing;

public class CookieEvents : CookieAuthenticationEvents
{
    private readonly IUserAccessTokenStore _store;

    public CookieEvents(IUserAccessTokenStore store)
    {
        _store = store;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var token = await _store.GetTokenAsync(context.Principal);
        if (token == null) context.RejectPrincipal();

        await base.ValidatePrincipal(context);
    }

}
