using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class SelectAccountViewModel
    {
        public string ReturnUrl { get; set; }

        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();
    }
}
