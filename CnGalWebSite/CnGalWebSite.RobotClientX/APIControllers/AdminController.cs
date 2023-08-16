using CnGalWebSite.RobotClientX.Services.QQClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace CnGalWebSite.RobotClientX.APIControllers
{
    [Route("api/admin/[action]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IQQClientService _QQClientService;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public AdminController(IQQClientService QQClientService, IHostApplicationLifetime applicationLifetime)
        {
            _QQClientService = QQClientService;
            _applicationLifetime = applicationLifetime;
        }

        /// <summary>
        /// 获取 Mirai 服务 Session
        /// </summary>
        /// <returns>Session</returns>
        [HttpGet]
        public async Task<string> GetMiraiSession()
        {
            return await Task.FromResult(_QQClientService.GetMiraiSession());
        }

        /// <summary>
        /// 重启
        /// </summary>
        /// <returns>Session</returns>
        [HttpPost]
        public async Task<ActionResult> Reboot()
        {
            _applicationLifetime.StopApplication();
            return await Task.FromResult(NotFound());
        }
    }
}
