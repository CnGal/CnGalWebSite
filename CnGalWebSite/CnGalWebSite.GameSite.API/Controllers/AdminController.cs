using CnGalWebSite.Core.Helpers;
using CnGalWebSite.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CnGalWebSite.GameSite.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        /// <summary>
        /// 获取服务器动态数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ServerRealTimeOverviewModel>> GetServerRealTimeDataOverview()
        {
            return await Task.FromResult(SystemEnvironmentHelper.GetServerRealTimeDataOverview());
        }

        /// <summary>
        /// 获取服务器静态数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ServerStaticOverviewModel>> GetServerStaticDataOverview()
        {
            return await Task.FromResult(SystemEnvironmentHelper.GetServerStaticDataOverview());
        }
    }
}
