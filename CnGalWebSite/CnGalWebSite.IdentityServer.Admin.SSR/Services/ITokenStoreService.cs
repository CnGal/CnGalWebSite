using CnGalWebSite.IdentityServer.Admin.SSR.Models;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Services
{
    public interface ITokenStoreService
    {
        Task<AppUserAccessToken> GetAsync(string id);

        Task SetAsync(AppUserAccessToken model);

        Task DeleteAsync(string id);
    }
}
