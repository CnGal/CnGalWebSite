using CnGalWebSite.Server.Models.Tokens;
using System.Threading.Tasks;

namespace CnGalWebSite.Server.Services
{
    public interface ITokenStoreService
    {
        Task<AppUserAccessToken> GetAsync(string id);

        Task SetAsync(AppUserAccessToken model);

        Task DeleteAsync(string id);
    }
}
