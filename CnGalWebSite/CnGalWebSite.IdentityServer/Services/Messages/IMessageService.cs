using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Messages
{
    public interface IMessageService
    {
        Task<bool> SendVerificationEmailAsync(int code, string email, ApplicationUser user, VerificationCodeType type);

        Task<bool> SendVerificationSMSAsync(int code, string phoneNumber, ApplicationUser user, VerificationCodeType type);
    }
}
