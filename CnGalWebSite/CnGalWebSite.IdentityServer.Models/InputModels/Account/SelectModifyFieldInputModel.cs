using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;

namespace CnGalWebSite.IdentityServer.Models.InputModels.Account
{
    public class SelectModifyFieldInputModel
    {
        public string ReturnUrl { get; set; }
        public string SecondCode { get; set; }

        public HumanMachineVerificationResult VerifyResult { get; set; } = new HumanMachineVerificationResult();

    }
}
