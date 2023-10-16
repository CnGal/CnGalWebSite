using CnGalWebSite.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.API.DataReositories;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.ProjectSite.API.Services.Users;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.ProjectSite.Models.ViewModels.Share;
using CnGalWebSite.ProjectSite.API.Services.Projects;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using CnGalWebSite.ProjectSite.API.Services.Messages;
using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IRepository<Project, long> _projectRepository;
        private readonly IRepository<ProjectPosition, long> _projectPositionRepository;
        private readonly IRepository<ProjectPositionUser, long> _projectPositionUserRepository;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IQueryService _queryService;
        private readonly IMessageService _messageService;

        public ProjectController(IRepository<Project, long> projectRepository, IUserService userService, IQueryService queryService, IProjectService projectService, IRepository<ProjectPosition, long> projectPositionRepository,
            IRepository<ProjectPositionUser, long> projectPositionUserRepository, IMessageService messageService)
        {
            _projectRepository = projectRepository;
            _userService = userService;
            _queryService = queryService;
            _projectService = projectService;
            _projectPositionRepository = projectPositionRepository;
            _projectPositionUserRepository = projectPositionUserRepository;
            _messageService = messageService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ProjectViewModel>> GetAsync([FromQuery] long id)
        {
            var user = await _userService.GetCurrentUserAsync();

            var item = await _projectRepository.GetAll().AsNoTracking()
                .Include(s=>s.CreateUser)
                .Include(s => s.Images)
                .Include(s => s.Positions).ThenInclude(s=>s.Users).ThenInclude(s=>s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ProjectViewModel
            {
                Contact = item.Positions.Any(s => s.Users.Any(s => s.UserId == user?.Id && s.Passed == true)) ? item.Contact : null,
                Description = item.Description,
                Name = item.Name,
                EndTime = item.EndTime,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime,
                Id = id,
                Images = item.Images.Select(s => new ProjectImageViewModel
                {
                    Image = s.Image,
                    Note = s.Note,
                    Priority = s.Priority,
                }).ToList(),

                Positions = item.Positions.Where(s => s.Hide == false).Select(s => new ProjectPositionViewModel
                {
                    BudgetNote = s.BudgetNote,
                    DeadLine = s.DeadLine,
                    Description = s.Description,
                    PositionType = s.PositionType,
                    PositionTypeName = s.PositionTypeName,
                    BudgetMax = s.BudgetMax,
                    BudgetMin = s.BudgetMin,
                    BudgetType = s.BudgetType,
                    Percentage = s.Percentage,
                    UrgencyType = s.UrgencyType,
                    Type = s.Type,
                    Id = s.Id,
                    Users = s.Users.Select(s => new ProjectPositionUserViewModel
                    {
                        User = _userService.GetUserInfo(s.User),
                        Id = s.Id,
                        Passed = s.Passed,
                        Contact = item.CreateUserId == user?.Id && s.Passed == true ? s.User.Contact : null
                    }).ToList()
                }).ToList(),
                CreateUser = _userService.GetUserInfo(item.CreateUser)
            };

            return model;
        }


        [HttpGet]
        public async Task<ActionResult<ProjectEditModel>> EditAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new ProjectEditModel
                {
                    EndTime = DateTime.Now.ToCstTime().AddDays(60)
                };
            }

            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _projectRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Positions)
                .FirstOrDefaultAsync(s => s.Id == id && (s.CreateUserId == user.Id || admin));

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ProjectEditModel
            {
                Contact = item.Contact,
                Description = item.Description,
                Name = item.Name,
                EndTime = item.EndTime,
                Id = id,
                Images = item.Images.Select(s => new ProjectImageEditModel
                {
                    Id = s.Id,
                    Image = s.Image,
                    Note = s.Note,
                    Priority = s.Priority,
                }).ToList(),

                Positions = item.Positions.Select(s => new ProjectPositionEditModel
                {
                    BudgetNote = s.BudgetNote,
                    DeadLine = s.DeadLine,
                    Description = s.Description,
                    PositionType = s.PositionType,
                    PositionTypeName = s.PositionTypeName,
                    BudgetMax = s.BudgetMax,
                    BudgetMin = s.BudgetMin,
                    BudgetType = s.BudgetType,
                    Id = s.Id,
                    Percentage = s.Percentage,
                    UrgencyType = s.UrgencyType,
                    Type = s.Type,
                    Hide=s.Hide
                }).ToList()
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(ProjectEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Success)
            {
                return vail;
            }
            var user = await _userService.GetCurrentUserAsync();

            Project item = null;
            if (model.Id == 0)
            {
                item = await _projectRepository.InsertAsync(new Project
                {
                    Description = model.Description,
                    Name = model.Name,
                    Contact = model.Contact,
                    EndTime = model.EndTime,
                    CreateTime = DateTime.Now.ToCstTime(),
                    CreateUserId = user.Id
                });
                model.Id = item.Id;
                _projectRepository.Clear();
            }

            var admin = _userService.CheckCurrentUserRole("Admin");

            item = await _projectRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Positions)
                .FirstOrDefaultAsync(s => s.Id == model.Id && (s.CreateUserId == user.Id || admin));

            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.Description = model.Description;
            item.Name = model.Name;
            item.Contact = model.Contact;
            item.EndTime = model.EndTime;

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
                }
            }
            item.Images.AddRange(model.Images.Where(s => s.Id == 0).Select(s => new ProjectImage
            {
                Image = s.Image,
                Note = s.Note,
                Priority = s.Priority,
            }));

            //职位
            item.Positions.RemoveAll(s => model.Positions.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Positions)
            {
                var temp = model.Positions.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.DeadLine = temp.DeadLine;
                    info.Description = temp.Description;
                    info.PositionType = temp.PositionType;
                    info.PositionTypeName = temp.PositionTypeName;
                    info.Type = temp.Type;
                    info.BudgetMax = temp.BudgetMax;
                    info.BudgetMin = temp.BudgetMin;
                    info.BudgetType = temp.BudgetType;
                    info.Id = temp.Id;
                    info.Percentage = temp.Percentage;
                    info.UrgencyType = temp.UrgencyType;
                    info.BudgetNote = temp.BudgetNote;
                    info.Hide = temp.Hide;
                    info.UpdateTime = DateTime.Now.ToCstTime();
                }
            }
            item.Positions.AddRange(model.Positions.Where(s => s.Id == 0).Select(s => new ProjectPosition
            {
                DeadLine = s.DeadLine,
                Description = s.Description,
                PositionType = s.PositionType,
                PositionTypeName = s.PositionTypeName,
                Type = s.Type,
                BudgetMax = s.BudgetMax,
                BudgetMin = s.BudgetMin,
                BudgetType = s.BudgetType,
                Id = s.Id,
                Percentage = s.Percentage,
                UrgencyType = s.UrgencyType,
                BudgetNote = s.BudgetNote,
                Hide = s.Hide,
                UpdateTime = DateTime.Now.ToCstTime(),
                CreateTime = DateTime.Now.ToCstTime(),
                ProjectId = model.Id,
            }));

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _projectRepository.UpdateAsync(item);

            return new Result { Success = true ,Message=model.Id.ToString()};
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<ProjectOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Project, long>(_projectRepository.GetAll().AsSingleQuery().Include(s=>s.CreateUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.CreateUser.UserName.Contains(model.SearchText)));

            return new QueryResultModel<ProjectOverviewModel>
            {
                Items = await items.Select(s => new ProjectOverviewModel
                {
                    Id = s.Id,
                    CreateTime = s.CreateTime,
                    EndTime = s.EndTime,
                    Name = s.Name,
                    UserId = s.CreateUserId,
                    UserName = s.CreateUser.UserName,
                    Hide=s.Hide,
                    Priority = s.Priority,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HideAsync(HideModel model)
        {
            await _projectRepository.GetAll().Where(s => model.Id==s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Hide, b => model.Hide));
            return new Result { Success = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPriorityAsync(EditPriorityModel model)
        {
            await _projectRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<List<ProjectInfoViewModel>> GetAll()
        {
            var now = DateTime.Now.ToCstTime();
            var projects = await _projectRepository.GetAll()
                .Where(s => s.Priority > 0 && s.Hide == false&&s.EndTime> now)
                .Include(s => s.Positions)
                .Include(s=>s.CreateUser)
                 .OrderByDescending(s => s.Priority)
                .ToListAsync();

            return projects.Select(s => _projectService.GetProjectInfoViewModel(s)).ToList();
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<List<ProjectPositionInfoViewModel>> GetAllPositions()
        {
            var now = DateTime.Now.ToCstTime();
            var projects = await _projectPositionRepository.GetAll()
                .Where(s => s.Priority > 0 && s.Hide == false && s.DeadLine > now)
                .Include(s => s.Project).ThenInclude(s => s.CreateUser)
                .OrderByDescending(s=>s.Priority)
                .ToListAsync();

            return projects.Select(s => _projectService.GetProjectPositionInfoViewModel(s)).ToList();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<ProjectPositionOverviewModel>> ListPositions(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ProjectPosition, long>(_projectPositionRepository.GetAll().AsSingleQuery().Include(s => s.Project), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Description.Contains(model.SearchText) || s.Project.Name.Contains(model.SearchText)));

            return new QueryResultModel<ProjectPositionOverviewModel>
            {
                Items = await items.Select(s => new ProjectPositionOverviewModel
                {
                    Id = s.Id,
                    CreateTime = s.CreateTime,
                    DeadLine = s.DeadLine,
                    Description = s.Description,
                    PositionType = s.PositionType,
                    PositionTypeName = s.PositionTypeName,
                    Type = s.Type,
                    ProjectId=s.ProjectId,
                    ProjectName=s.Project.Name,
                    Hide = s.Hide,
                    Priority = s.Priority,
                    Tags = s.Tags,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HidePositionAsync(HideModel model)
        {
            await _projectPositionRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Hide, b => model.Hide));
            return new Result { Success = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPositionPriorityAsync(EditPriorityModel model)
        {
            await _projectPositionRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<Result> ChangeTagsAsync(ProjectPositionChangeTagsModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            if (!admin)
            {
                return new Result { Success = false, Message = "权限不足" };
            }

            await _projectPositionRepository.GetAll().Where(s => s.Id == model.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.Tags, b => model.Tags));

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<Result> ApplyProjectPosition(ApplyProjectPositionModel model)
        {
            var user = await _userService.GetCurrentUserAsync();

            var position = await _projectPositionRepository.GetAll().AsNoTracking().Include(s=>s.Project).FirstOrDefaultAsync(s => s.Id == model.PositionId);
            if (position == null)
            {
                return new Result { Success = false, Message = "找不到这个约稿" };
            }

            if(position.DeadLine<DateTime.Now.ToCstTime())
            {
                return new Result { Success = false, Message = "已经过了截止日期" };
            }

            if(await _projectPositionUserRepository.AnyAsync(s=>s.PositionId==model.PositionId&&s.UserId==user.Id))
            {
                if(model.Apply)
                {
                    return new Result { Success = false, Message = "你已经应征了" };
                }
                else
                {
                    await _projectPositionUserRepository.DeleteAsync(s => s.PositionId == model.PositionId && s.UserId == user.Id);

                    //向用户发送消息
                    await _messageService.PutMessage(new PutMessageModel
                    {
                        PageId = position.Project.Id,
                        PageType = PageType.Project,
                        Text = $"“{user.GetName()}”取消应征你的企划“{position.Project.Name}”",
                        Type = MessageType.ApplyProjectPosition,
                        UserId = position.Project.CreateUserId,
                    });

                    return new Result { Success = true };
                }
            }

            await _projectPositionUserRepository.InsertAsync(new ProjectPositionUser
            {
                PositionId = model.PositionId,
                UserId = user.Id,
            });

            //向用户发送消息
            await _messageService.PutMessage(new PutMessageModel
            {
                PageId = position.Project.Id,
                PageType = PageType.Project,
                Text = $"“{user.GetName()}”应征你的企划“{position.Project.Name}”",
                Type = MessageType.ApplyProjectPosition,
                UserId = position.Project.CreateUserId,
            });

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<Result> ProcProjectPosition(ProcProjectPositionModel model)
        {
            var user = await _userService.GetCurrentUserAsync();

            var positionUser = await _projectPositionUserRepository.GetAll()
                .Include(s=>s.Position).ThenInclude(s=>s.Project)
                .FirstOrDefaultAsync(s => s.Id == model.UserId);

            if (positionUser == null)
            {
                return new Result { Success = false, Message = "找不到这个应征用户" };
            }

            if(positionUser.Position.Project.CreateUserId!=user.Id&&! _userService.CheckCurrentUserRole("Admin"))
            {
                return new Result { Success = false, Message = "权限不足" };
            }

            positionUser.Passed = model.Passed;

            await _projectPositionUserRepository.UpdateAsync(positionUser);

            //向用户发送消息
            await _messageService.PutMessage(new PutMessageModel
            {
                PageId = positionUser.Position.Project.Id,
                PageType = PageType.Project,
                Text = $"应征的企划“{positionUser.Position.Project.Name}”{(model.Passed==true?"选定":"取消选定")}了你",
                Type = MessageType.PassedPositionUser,
                UserId = positionUser.UserId,
            });


            return new Result { Success = true };
        }
    }
}
