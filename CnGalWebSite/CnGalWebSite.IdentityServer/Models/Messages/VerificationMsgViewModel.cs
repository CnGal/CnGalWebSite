using CnGalWebSite.IdentityServer.Models.Account;

namespace CnGalWebSite.IdentityServer.Models.Messages
{
    public class VerificationMsgViewModel
    {
        public string UserName { get; set; }

        public int Code { get; set; }

        public string Purpose { get; set; }
    }
}
