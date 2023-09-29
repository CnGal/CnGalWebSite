using CnGalWebSite.Server.Models.Tokens;

namespace CnGalWebSite.ProjectSite.SSR.Services
{
    public interface ITokenStoreService
    {
        Task<AppUserAccessToken> GetAsync(string id);

        Task SetAsync(AppUserAccessToken model);

        Task DeleteAsync(string id);
    }
}
