﻿using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.BlazorWeb.Plumbing;

public class OidcEvents : OpenIdConnectEvents
{
    private readonly IUserAccessTokenStore _store;

    public OidcEvents(IUserAccessTokenStore store)
    {
        _store = store;
    }
    
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(Double.Parse(context.TokenEndpointResponse!.ExpiresIn));

        await _store.StoreTokenAsync(
            context.Principal!, 
            context.TokenEndpointResponse.AccessToken, 
            exp,
            context.TokenEndpointResponse.RefreshToken);
        
        await base.TokenValidated(context);
    }
}
