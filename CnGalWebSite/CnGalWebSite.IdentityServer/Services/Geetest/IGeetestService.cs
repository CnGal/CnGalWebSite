using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;

namespace CnGalWebSite.IdentityServer.Services.Geetest
{
    public interface IGeetestService
    {
        GeetestCodeModel GetGeetestCode(GeetestScenario scenario);

        bool CheckRecaptcha(HumanMachineVerificationResult model, GeetestScenario scenario);
    }
}
