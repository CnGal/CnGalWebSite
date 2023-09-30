using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.API.Services.Users;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IRepository<ApplicationUser, string> _userRepository;

        public UserController(ILogger<UserController> logger, IUserService userService, IRepository<ApplicationUser, string> userRepository)
        {
            _logger = logger;
            _userService = userService;
            _userRepository= userRepository;
        }

        [HttpGet]
        public async Task<List<KeyValuePair<string, string>>> GetUserClaims()
        {
            return await Task.FromResult(User?.Claims?.Select(s => new KeyValuePair<string, string>(s.Type, s.Value))?.ToList());
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<UserInfoViewModel>> GetUserInfo([FromQuery]string id)
        {
            ApplicationUser user = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                user =await _userService.GetCurrentUserAsync();
            }
            else
            {
                user=await _userRepository.FirstOrDefaultAsync(s=>s.Id == id);
            }

            if(user == null)
            {
                return NotFound("该用户不存在");
            }

            return new UserInfoViewModel
            {
                Avatar = user.Avatar,
                BackgroundImage = user.BackgroundImage,
                BriefIntroduction = user.BriefIntroduction,
                Id = user.Id,
                Name = user.UserName,
                RegistTime = user.RegistTime
            };
        }

        [HttpGet]
        public async Task<ActionResult<UserSpaceViewModel>> GetUserSpace()
        {
            return new UserSpaceViewModel();
        }
    }
}
