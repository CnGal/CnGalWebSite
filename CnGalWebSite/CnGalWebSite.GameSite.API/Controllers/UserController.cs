using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CnGalWebSite.GameSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public async Task<List<KeyValuePair<string, string>>> GetUserClaims()
        {
            return await Task.FromResult(User?.Claims?.Select(s => new KeyValuePair<string, string>(s.Type, s.Value))?.ToList());
        }

    }
}
