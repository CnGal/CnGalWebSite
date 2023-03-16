using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class ChangePhoneNumberViewModel:ChangePhoneNumberInputModel
    {
        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }
}
