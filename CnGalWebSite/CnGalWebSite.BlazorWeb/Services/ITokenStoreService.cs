using CnGalWebSite.BlazorWeb.Models.Tokens;
using System.Threading.Tasks;

namespace CnGalWebSite.BlazorWeb.Services
{
    public interface ITokenStoreService
    {
        Task<AppUserAccessToken> GetAsync(string id);

        Task SetAsync(AppUserAccessToken model);

        Task DeleteAsync(string id);
    }
}
