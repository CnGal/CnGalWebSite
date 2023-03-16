using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class ChangeEmailViewModel:ChangeEmailInputModel
    {
        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }
}
