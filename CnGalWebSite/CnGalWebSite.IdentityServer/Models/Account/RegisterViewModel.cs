using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class RegisterViewModel:RegisterInputModel
    {
        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }
}
