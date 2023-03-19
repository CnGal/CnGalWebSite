using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CnGalWebSite.IdentityServer.Admin.SSR.Controllers
{
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        public IActionResult LogIn()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/"
            };

            return Challenge(props);
        }

        public IActionResult LogOut()
        {
            return SignOut("cookie", "oidc");
        }
    }
}
