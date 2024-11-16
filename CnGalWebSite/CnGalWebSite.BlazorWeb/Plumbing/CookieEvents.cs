using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace CnGalWebSite.BlazorWeb.Plumbing;

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
