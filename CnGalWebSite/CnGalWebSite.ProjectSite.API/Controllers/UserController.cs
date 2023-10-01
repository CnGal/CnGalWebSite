using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.API.Services.Users;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.Core.Models;
using System.Xml.Linq;
using CnGalWebSite.ProjectSite.API.Services.Projects;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IRepository<ApplicationUser, string> _userRepository;

        public UserController(ILogger<UserController> logger, IUserService userService, IRepository<ApplicationUser, string> userRepository, IProjectService projectService)
        {
            _logger = logger;
            _userService = userService;
            _userRepository = userRepository;
            _projectService=projectService;
        }

        [HttpGet]
        public async Task<List<KeyValuePair<string, string>>> GetUserClaims()
        {
            return await Task.FromResult(User?.Claims?.Select(s => new KeyValuePair<string, string>(s.Type, s.Value))?.ToList());
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<UserInfoViewModel>> GetUserInfo([FromQuery] string id)
        {
            var user = await _userService.GetUserInfo(id);

            if (user == null)
            {
                return NotFound("该用户不存在");
            }

            return user;
        }

        [HttpGet]
        public async Task<ActionResult<UserEditModel>> EditAsync([FromQuery] string id)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _userRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == id && (s.Id == user.Id || admin));

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new UserEditModel
            {
                Avatar = item.Avatar,
                BackgroundImage = item.BackgroundImage,
                Name = item.UserName,
                OrganizationDescription = item.OrganizationDescription,
                OrganizationName = item.OrganizationName,
                PersonDescription = item.PersonDescription,
                PersonName = item.PersonName,
                Type = item.Type,
                Id = item.Id
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(UserEditModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _userRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == model.Id && (s.Id == user.Id || admin));

            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.Avatar = model.Avatar;
            item.BackgroundImage = model.BackgroundImage;
            item.UserName = model.Name;
            item.OrganizationDescription = model.OrganizationDescription;
            item.OrganizationName = model.OrganizationName;
            item.PersonDescription = model.PersonDescription;
            item.PersonName = model.PersonName;

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _userRepository.UpdateAsync(item);

            return new Result { Success = true, Message = model.Id };
        }

        [HttpPost]
        public async Task<Result> SwitchTypeAsync(UserSwitchTypeModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            if (model.Id != user.Id &&! admin)
            {
                return new Result { Success = false, Message = "权限不足" };
            }

            await _userRepository.GetAll().Where(s => s.Id == model.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Type, b => model.Type));

            return new Result { Success = true };
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<UserSpaceViewModel>> GetUserSpace([FromQuery] string id)
        {
            var userinfo = await _userService.GetUserInfo(id);

            if (userinfo == null)
            {
                return NotFound("该用户不存在");
            }

           var user = await _userRepository.GetAll().AsNoTracking()
                .Include(s => s.Projects).ThenInclude(s=>s.Positions)
                .FirstOrDefaultAsync(s => s.Id == userinfo.Id);

            return new UserSpaceViewModel
            {
                UserInfo = userinfo,
                projects = user.Projects.Select(s => _projectService.GetProjectInfoViewModel(s)).ToList()
            };
        }
    }
}
