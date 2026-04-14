namespace CnGalWebSite.IdentityServer.Models.DataModels.Geetest
{
    public class HumanMachineVerificationResult
    {
        public string LotNumber { get; set; }

        public string CaptchaOutput { get; set; }

        public string PassToken { get; set; }

        public string GenTime { get; set; }
    }
}
