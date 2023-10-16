using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;
using CnGalWebSite.Extensions;
using CnGalWebSite.GameSite.SSR.Models.Tokens;
using CnGalWebSite.GameSite.SSR.Services;
using IdentityModel.AspNetCore.AccessTokenManagement;

namespace CnGalWebSite.GameSite.SSR.Plumbing;

/// <summary>
/// Simplified implementation of a server-side token store.
/// Probably want somehting more robust IRL
/// </summary>
public class ServerSideTokenStore : IUserAccessTokenStore
{
    private readonly ITokenStoreService _tokenStoreService;

    public ServerSideTokenStore(ITokenStoreService tokenStoreService)
    {
        _tokenStoreService = tokenStoreService;
    }

    public async Task ClearTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters? parameters = null)
    {
        var sub = user?.Claims?.GetUserId() ?? throw new InvalidOperationException("no sub claim");

        await _tokenStoreService.DeleteAsync(sub);
    }

    public async Task<UserAccessToken?> GetTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters? parameters = null)
    {
        var sub = user?.Claims?.GetUserId() ?? throw new InvalidOperationException("no sub claim");

        if (string.IsNullOrWhiteSpace(sub))
        {
            return null;
        }

        var value = await _tokenStoreService.GetAsync(sub);
        if (value == null)
        {
            return null;
        }
        else
        {
            return new UserAccessToken
            {
                AccessToken = value.AccessToken,
                Expiration = value.Expiration,
                RefreshToken = value.RefreshToken,
            };
        }

    }

    public async Task StoreTokenAsync(ClaimsPrincipal user, string accessToken, DateTimeOffset expiration, string? refreshToken = null, UserAccessTokenParameters? parameters = null)
    {
        var sub = user?.Claims?.GetUserId() ?? throw new InvalidOperationException("no sub claim");

        await _tokenStoreService.SetAsync(new AppUserAccessToken
        {
            UserId = sub,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = expiration,
        });
    }
}
