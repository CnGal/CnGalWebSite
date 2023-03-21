using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Geetest
{
    public interface IGeetestService
    {
        GeetestCodeModel GetGeetestCode(ControllerBase controller);

        bool CheckRecaptcha(HumanMachineVerificationResult model);
    }
}
