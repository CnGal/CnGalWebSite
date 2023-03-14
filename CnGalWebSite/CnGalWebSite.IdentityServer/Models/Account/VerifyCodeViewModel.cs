namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class VerifyCodeViewModel : VerifyCodeInputModel
    {
        public bool IsDisableRepost { get; set; }
        public bool IsPhoneNumber { get { return Type.IsPhoneNumber(); } }
    }
}
