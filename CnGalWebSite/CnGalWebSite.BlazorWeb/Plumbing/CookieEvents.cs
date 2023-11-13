using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace CnGalWebSite.BlazorWeb.Plumbing;

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
