using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Account
{
    public interface IAccountService
    {
        Task<(ApplicationUser user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProviderAsync(AuthenticateResult result);

        Task<List<ExternalProvider>> GetExternalProvidersAsync(AuthorizationRequest context);

        Task<AccountBindInfor> GetAccountBindInforAsync(AuthorizationRequest context, ApplicationUser user);

        Task<bool> IsNewUserAsync(string id);
    }
}
