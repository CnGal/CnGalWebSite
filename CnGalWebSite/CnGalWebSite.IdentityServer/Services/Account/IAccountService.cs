using CnGalWebSite.IdentityServer.Models.Account;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Account
{
    public interface IAccountService
    {
        Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult result);
    }
}
