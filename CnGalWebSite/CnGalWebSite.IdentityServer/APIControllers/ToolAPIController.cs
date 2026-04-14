using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using CnGalWebSite.IdentityServer.Services.Geetest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.APIControllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/tool/[action]")]
    public class ToolAPIController : ControllerBase
    {
        private readonly IGeetestService _geetestService;

        public ToolAPIController(IGeetestService geetestService)
        {
            _geetestService = geetestService;
        }

        [HttpGet]
        public async Task<GeetestCodeModel> GetGeetestCode([FromQuery] string scenario = "Login")
        {
            if (!Enum.TryParse<GeetestScenario>(scenario, true, out var geetestScenario))
            {
                geetestScenario = GeetestScenario.Login;
            }
            return await Task.FromResult(_geetestService.GetGeetestCode(geetestScenario));
        }
    }
}
