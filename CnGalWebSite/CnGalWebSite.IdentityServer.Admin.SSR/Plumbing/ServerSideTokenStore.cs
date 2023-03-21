using System.Collections.Concurrent;
using System.Security.Claims;
using CnGalWebSite.Extensions;
using CnGalWebSite.IdentityServer.Admin.SSR.Models;
using CnGalWebSite.IdentityServer.Data;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.EntityFrameworkCore;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Plumbing;

/// <summary>
/// Simplified implementation of a server-side token store.
/// Probably want somehting more robust IRL
/// </summary>
public class ServerSideTokenStore : IUserAccessTokenStore
{
    private readonly ApplicationDbContext _context;

    public ServerSideTokenStore(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ClearTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters? parameters = null)
    {
        var sub = user?.Claims?.GetUserId() ?? throw new InvalidOperationException("no sub claim");

        await _context.AppUserAccessTokens.Where(s => s.UserId == sub).ExecuteDeleteAsync();
    }

    public async Task<UserAccessToken?> GetTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters? parameters = null)
    {
        var sub = user?.Claims?.GetUserId() ?? throw new InvalidOperationException("no sub claim");

        var value =await _context.AppUserAccessTokens.FirstOrDefaultAsync(s => s.UserId == sub);
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

        await _context.AppUserAccessTokens.Where(s => s.UserId == sub).ExecuteDeleteAsync();
        await _context.AppUserAccessTokens.AddAsync(new AppUserAccessToken
        {
            UserId = sub,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = expiration,
        });
        await _context.SaveChangesAsync();
    }

}
