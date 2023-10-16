using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using System.Threading.Tasks;

namespace CnGalWebSite.GameSite.SSR.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
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

            //跳转到Identity Server 4统一退出登录
            return SignOut(properties, "cngal");
        }
    }
}
