using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using CnGalWebSite.IdentityServer.Models.InputModels.Account;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Account
{
    public class VerifyCodeViewModel : VerifyCodeInputModel
    {
        public bool IsDisableRepost { get; set; }
        public bool IsPhoneNumber { get { return Type.IsPhoneNumber(); } }

        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }
}
