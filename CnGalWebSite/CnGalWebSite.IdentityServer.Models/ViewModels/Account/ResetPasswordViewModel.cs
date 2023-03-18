using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using CnGalWebSite.IdentityServer.Models.InputModels.Account;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Account
{
    public class ResetPasswordViewModel : ResetPasswordInputModel
    {
        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }
}
