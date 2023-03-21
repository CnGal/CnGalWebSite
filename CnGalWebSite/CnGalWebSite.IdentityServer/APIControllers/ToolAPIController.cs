using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using CnGalWebSite.IdentityServer.Services.Account;
using CnGalWebSite.IdentityServer.Services.Geetest;
using CnGalWebSite.IdentityServer.Services.Messages;
using IdentityModel;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

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
        public async Task<GeetestCodeModel> GetGeetestCode()
        {
            return await Task.FromResult(_geetestService.GetGeetestCode(this));
        }
    }
}
