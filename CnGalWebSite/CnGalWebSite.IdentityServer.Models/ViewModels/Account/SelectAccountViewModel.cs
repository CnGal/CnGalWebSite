using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Account
{
    public class SelectAccountViewModel
    {
        public string ReturnUrl { get; set; }

        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();
    }
}
