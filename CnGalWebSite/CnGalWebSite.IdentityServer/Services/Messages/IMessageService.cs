using CnGalWebSite.IdentityServer.Models.Account;
using CnGalWebSite.IdentityServer.Models.Messages;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Messages
{
    public interface IMessageService
    {
        Task<bool> SendVerificationEmailAsync(int code, string email, ApplicationUser user, VerificationCodeType type);

        Task<bool> SendVerificationSMSAsync(int code, ApplicationUser user, VerificationCodeType type);
    }
}
