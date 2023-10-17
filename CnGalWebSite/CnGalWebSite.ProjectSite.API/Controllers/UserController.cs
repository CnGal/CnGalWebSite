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
using CnGalWebSite.ProjectSite.API.Services.Stalls;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;
using System.Collections.Generic;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.Core.Services.Query;

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
        private readonly IStallService _stallService;
        private readonly IQueryService _queryService;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<ProjectPositionUser, long> _projectPositionUserRepository;

        public UserController(ILogger<UserController> logger, IUserService userService, IRepository<ApplicationUser, string> userRepository, IProjectService projectService, IStallService stallService, IQueryService queryService,
            IRepository<ProjectPositionUser, long> projectPositionUserRepository)
        {
            _logger = logger;
            _userService = userService;
            _userRepository = userRepository;
            _projectService=projectService;
            _stallService = stallService;
            _queryService = queryService;
            _projectPositionUserRepository = projectPositionUserRepository;
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
                .Include(s => s.Images)
                .Include(s => s.Texts)
                .Include(s => s.Audios)
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
                Id = item.Id,
                Contact = item.Contact,
                Images = item.Images.Select(s => new UserImageEditModel
                {
                    Id = s.Id,
                    Image = s.Image,
                    Note = s.Note,
                    Priority = s.Priority,
                }).ToList(),
                Audios = item.Audios.Select(s => new EditAudioAloneModel
                {
                    BriefIntroduction = s.BriefIntroduction,
                    Duration = s.Duration,
                    Id = s.Id,
                    Name = s.Name,
                    Priority = s.Priority,
                    Thumbnail = s.Thumbnail,
                    Url = s.Url,
                }).ToList(),
                Texts = item.Texts.Select(s => new UserTextEditModel
                {
                    Name = s.Name,
                    Content = s.Content,
                    Link=s.Link,
                    Id = s.Id
                }).ToList(),
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(UserEditModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _userRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Texts)
                .Include(s => s.Audios)
                .FirstOrDefaultAsync(s => s.Id == model.Id && (s.Id == user.Id || admin));

            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }
            if(await _userRepository.AnyAsync(s=>s.Id!=model.Id&&s.UserName==model.Name))
            {
                return new Result { Success = false, Message = "用户名重复" };
            }

            item.Avatar = model.Avatar;
            item.BackgroundImage = model.BackgroundImage;
            item.UserName = model.Name;
            item.OrganizationDescription = model.OrganizationDescription;
            item.OrganizationName = model.OrganizationName;
            item.PersonDescription = model.PersonDescription;
            item.PersonName = model.PersonName;
            item.Contact = model.Contact;

            //相册
            item.Images.RemoveAll(s => model.Images.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Images)
            {
                var temp = model.Images.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.Image = temp.Image;
                    info.Note = temp.Note;
                    info.Priority = temp.Priority;
                    info.UpdateTime = DateTime.Now.ToCstTime();
                }
            }
            item.Images.AddRange(model.Images.Where(s => s.Id == 0).Select(s => new UserImage
            {
                Image = s.Image,
                Note = s.Note,
                Priority = s.Priority,
                UpdateTime = DateTime.Now.ToCstTime(),
                CreateTime = DateTime.Now.ToCstTime(),
                CreateUserId = user.Id,
            }));
            //音频
            item.Audios.RemoveAll(s => model.Audios.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Audios)
            {
                var temp = model.Audios.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.BriefIntroduction = temp.BriefIntroduction;
                    info.Duration = temp.Duration;
                    info.Name = temp.Name;
                    info.Priority = temp.Priority;
                    info.Thumbnail = temp.Thumbnail;
                    info.Url = temp.Url;
                    info.UpdateTime = DateTime.Now.ToCstTime();
                }
            }
            item.Audios.AddRange(model.Audios.Where(s => s.Id == 0).Select(s => new UserAudio
            {
                BriefIntroduction = s.BriefIntroduction,
                Duration = s.Duration,
                Id = s.Id,
                Name = s.Name,
                Priority = s.Priority,
                Thumbnail = s.Thumbnail,
                Url = s.Url,
                UpdateTime = DateTime.Now.ToCstTime(),
                CreateTime = DateTime.Now.ToCstTime(),
                CreateUserId = user.Id,
            }));
            //文本
            item.Texts.RemoveAll(s => model.Texts.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Texts)
            {
                var temp = model.Texts.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.Name = temp.Name;
                    info.Content = temp.Content;
                    info.Link = temp.Link;
                    info.UpdateTime = DateTime.Now.ToCstTime();
                }
            }
            item.Texts.AddRange(model.Texts.Where(s => s.Id == 0).Select(s => new UserText
            {
                Name = s.Name,
                Content = s.Content,
                Link = s.Link,
                UpdateTime = DateTime.Now.ToCstTime(),
                CreateTime= DateTime.Now.ToCstTime(),
                CreateUserId=user.Id,
            }));

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _userRepository.UpdateAsync(item);

            return new Result { Success = true, Message = model.Id };
        }

        [HttpPost]
        public async Task<Result> SwitchTypeAsync(UserSwitchTypeModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            if (model.Id != user.Id && !admin)
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
                .Include(s=>s.Stalls).ThenInclude(s=>s.Images)
                .Include(s => s.Stalls).ThenInclude(s => s.Informations).ThenInclude(s => s.Type)
                .Include(s => s.Images)
                .Include(s => s.Texts)
                .Include(s => s.Audios)
                .FirstOrDefaultAsync(s => s.Id == userinfo.Id);

            var positions = await _projectPositionUserRepository.GetAll().AsNoTracking().Include(s => s.Position).ThenInclude(s=>s.Project).ThenInclude(s=>s.CreateUser).Where(s => s.UserId == user.Id && s.Passed==true).Select(s=>s.Position).ToListAsync();

            return new UserSpaceViewModel
            {
                TabIndex= user.Type== UserType.Person? 2 : 0,
                UserInfo = userinfo,
                Projects = user.Projects.Where(s=>s.Hide==false).Select(s => _projectService.GetProjectInfoViewModel(s)).ToList(),
                Stalls = user.Stalls.Where(s => s.Hide == false).Select(s => _stallService.GetStallInfoViewModel(s, user)).ToList(),
                Images = user.Images.Where(s => s.Hide == false).Select(s => new UserImageViewModel
                {
                    Image = s.Image,
                    Note = s.Note,
                    Priority = s.Priority,
                }).ToList(),
                Audios = user.Audios.Where(s => s.Hide == false).Select(s => new EditAudioAloneModel
                {
                    BriefIntroduction = s.BriefIntroduction,
                    Duration = s.Duration,
                    Id = s.Id,
                    Name = s.Name,
                    Priority = s.Priority,
                    Thumbnail = s.Thumbnail,
                    Url = s.Url,
                }).ToList(),
                Texts = user.Texts.Where(s => s.Hide == false).Select(s => new UserTextViewModel
                {
                    Name = s.Name,
                    Content = s.Content,
                    Link = s.Link,
                }).ToList(),
                Positions= positions.Select(s=>_projectService.GetProjectPositionInfoViewModel(s)).ToList(),
            };
        }

        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<QueryResultModel<UserOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ApplicationUser, string>(_userRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.UserName.Contains(model.SearchText) || s.Email.Contains(model.SearchText)));

            return new QueryResultModel<UserOverviewModel>
            {
                Items = await items.Select(s => new UserOverviewModel
                {
                    Id = s.Id,
                    Avatar = s.Avatar,
                    BackgroundImage = s.BackgroundImage,
                    Name = s.UserName,
                    RegistTime = s.RegistTime,
                    Email = s.Email,
                    Type = s.Type,
                    Tags=s.Tags,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
