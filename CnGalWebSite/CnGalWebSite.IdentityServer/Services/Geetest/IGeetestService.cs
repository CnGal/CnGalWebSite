using CnGalWebSite.IdentityServer.Models.Geetest;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Services.Geetest
{
    public interface IGeetestService
    {
        GeetestCodeModel GetGeetestCode(Controller controller);

        bool CheckRecaptcha(HumanMachineVerificationResult model);
    }
}
