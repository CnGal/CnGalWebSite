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

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IRepository<Project, long> _projectRepository;
        private readonly IUserService _userService;


        public ProjectController(IRepository<Project, long> projectRepository, IUserService userService)
        {
            _projectRepository = projectRepository;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<ProjectEditModel>> EditAsync([FromQuery]long id)
        {
            if(id==0)
            {
                return new ProjectEditModel();
            }

            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _projectRepository.GetAll()
                .Include(s=>s.Images)
                .Include(s=>s.Positions)
                .FirstOrDefaultAsync(s => s.Id == id && (s.CreateUserId == user.Id || admin));

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ProjectEditModel
            {
                BudgetRange = item.BudgetRange,
                Contact = item.Contact,
                Description = item.Description,
                EndTime = item.EndTime,
                Id = id,
                Images = item.Images.Select(s => new ProjectImageViewModel
                {
                    Id = s.Id,
                    Image = s.Image,
                    Note = s.Note,
                    Priority = s.Priority,
                }).ToList(),
                Name = item.Name,
                Positions = item.Positions.Select(s => new ProjectPositionViewModel
                {
                    BudgetRange = s.BudgetRange,
                    DeadLine = s.DeadLine,
                    Description = s.Description,
                    PositionType = s.PositionType,
                    PositionTypeName = s.PositionTypeName,
                    Type = s.Type,
                }).ToList()
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(ProjectEditModel model)
        {
            Project item = null;
            if (model.Id == 0)
            {
                item = await _projectRepository.InsertAsync(new Project
                {
                    Description = model.Description,
                    Name = model.Name,
                    BudgetRange = model.BudgetRange,
                    Contact = model.Contact,
                    EndTime = model.EndTime,
                    CreateTime = DateTime.Now.ToCstTime(),
                });
                model.Id = item.Id;
                _projectRepository.Clear();
            }

            var user = await _userService.GetCurrentUserAsync();
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
            item.BudgetRange = model.BudgetRange;
            item.Contact = model.Contact;
            item.EndTime = model.EndTime;

            //相册
            item.Images = model.Images.Select(s=>new ProjectImage
            {
                Id = s.Id,
                Image = s.Image,
                Note = s.Note,
                Priority = s.Priority,
            }).ToList();
            //职位
            item.Positions = model.Positions.Select(s=>new ProjectPosition
            {
                BudgetRange= s.BudgetRange,
                DeadLine=s.DeadLine,
                Description=s.Description,
                PositionType=s.PositionType,
                PositionTypeName=s.PositionTypeName,
                Type = s.Type,
            }).ToList();

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _projectRepository.UpdateAsync(item);

            return new Result { Success = true };
        }
    }
}
