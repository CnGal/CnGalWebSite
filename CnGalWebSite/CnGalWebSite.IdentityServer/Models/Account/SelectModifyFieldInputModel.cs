using CnGalWebSite.IdentityServer.Models.Geetest;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class SelectModifyFieldInputModel
    {
        public string ReturnUrl { get; set; }
        public string SecondCode { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();

    }
}
