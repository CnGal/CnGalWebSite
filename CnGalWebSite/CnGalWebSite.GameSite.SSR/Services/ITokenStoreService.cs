using CnGalWebSite.GameSite.SSR.Models.Tokens;

namespace CnGalWebSite.GameSite.SSR.Services
{
    public interface ITokenStoreService
    {
        Task<AppUserAccessToken> GetAsync(string id);

        Task SetAsync(AppUserAccessToken model);

        Task DeleteAsync(string id);
    }
}
