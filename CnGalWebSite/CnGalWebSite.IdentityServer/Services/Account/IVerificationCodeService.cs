using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Account
{
    public interface IVerificationCodeService
    {
        Task<int> GetCodeAsync(string token, string userName, VerificationCodeType type);

        Task<string> GetTokenAsync(string code, string userName, VerificationCodeType type);

        Task<bool> CheckAsync(string code, string userName, VerificationCodeType type);
    }
}
