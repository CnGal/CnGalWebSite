using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class ResetPasswordViewModel:ResetPasswordInputModel
    {
        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }
}
