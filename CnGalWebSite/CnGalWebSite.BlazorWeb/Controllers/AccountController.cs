using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using System.Threading.Tasks;

namespace CnGalWebSite.BlazorWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger _logger;
        private readonly IDataCacheService _dataCacheService;

        public AccountController(ILogger<AccountController> logger, IDataCacheService dataCacheService)
        {
            _logger = logger;
            _dataCacheService = dataCacheService;
        }

        /// <summary>
        /// 跳转到Identity Server 4统一登录
        /// </summary>
        /// <param name="returnUrl">登录成功后，返回之前的网页路由</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string returnUrl = "")
        {
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = "/";

            var properties = new AuthenticationProperties
            {
                //记住登录状态
                IsPersistent = true,

                RedirectUri = returnUrl
            };

            _logger.LogInformation($"id4跳转登录, returnUrl={returnUrl}");

            //跳转到Identity Server 4统一登录
            return Challenge(properties, "cngal");
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var userName = HttpContext.User.Identity?.Name;

            _logger.LogInformation($"{userName}退出登录。");

            //删除登录状态cookies
            await HttpContext.SignOutAsync("cookies");

            var properties = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            //清除缓存
            _dataCacheService.RefreshAllCatche();

            //跳转到Identity Server 4统一退出登录
            return SignOut(properties, "cngal");
        }
    }
}
