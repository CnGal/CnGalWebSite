using CnGalWebSite.ProjectSite.API.DataReositories;
using CnGalWebSite.ProjectSite.API.Services.Users;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.Core.Models;
using System.Xml.Linq;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using CnGalWebSite.Core.Services.Query;

namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StallController : ControllerBase
    {
        private readonly IRepository<Stall, long> _stallRepository;
        private readonly IUserService _userService;
        private readonly IQueryService _queryService;

        public StallController(IRepository<Stall, long> stallRepository, IUserService userService, IQueryService queryService)
        {
            _stallRepository = stallRepository;
            _userService = userService;
            _queryService= queryService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<StallViewModel>> GetAsync([FromQuery] long id)
        {
            var item = await _stallRepository.GetAll()
                .Include(s => s.Images)
                .Include(s=>s.Audios)
                .Include(s=>s.Texts)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new StallViewModel
            {
                Description = item.Description,
                Name = item.Name,
                EndTime = item.EndTime,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime,
                Contact = item.Contact,
                PositionType = item.PositionType,
                PositionTypeName = item.PositionTypeName,
                Price = item.Price,
                Type = item.Type,
                Id = id,
                Images = item.Images.Select(s => new StallImageViewModel
                {
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
                Texts = item.Texts.Select(s => new StallTextViewModel
                {
                    Name = s.Name,
                    Content=s.Content
                }).ToList(),
                CreateUser = await _userService.GetUserInfo(item.CreateUserId)
            };

            return model;
        }


        [HttpGet]
        public async Task<ActionResult<StallEditModel>> EditAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new StallEditModel
                {
                    EndTime = DateTime.Now.ToCstTime().AddDays(60)
                };
            }

            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _stallRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Audios)
                .Include(s => s.Texts)
                .FirstOrDefaultAsync(s => s.Id == id && (s.CreateUserId == user.Id || admin));

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new StallEditModel
            {
                Contact = item.Contact,
                Description = item.Description,
                Name = item.Name,
                EndTime = item.EndTime,
                PositionType = item.PositionType,
                PositionTypeName = item.PositionTypeName,
                Price = item.Price,
                Type = item.Type,
                Id = id,
                Images = item.Images.Select(s => new StallImageEditModel
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
                Texts = item.Texts.Select(s => new StallTextEditModel
                {
                    Name = s.Name,
                    Content = s.Content,
                    Id=s.Id
                }).ToList(),
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(StallEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Success)
            {
                return vail;
            }
            var user = await _userService.GetCurrentUserAsync();

            Stall item = null;
            if (model.Id == 0)
            {
                item = await _stallRepository.InsertAsync(new Stall
                {
                    Description = model.Description,
                    Name = model.Name,
                    Contact = model.Contact,
                    EndTime = model.EndTime,
                    PositionType = model.PositionType,
                    PositionTypeName = model.PositionTypeName,
                    Price = model.Price,
                    Type = model.Type,
                    CreateTime = DateTime.Now.ToCstTime(),
                    CreateUserId = user.Id
                });
                model.Id = item.Id;
                _stallRepository.Clear();
            }

            var admin = _userService.CheckCurrentUserRole("Admin");

            item = await _stallRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Audios)
                .Include(s => s.Texts)
                .FirstOrDefaultAsync(s => s.Id == model.Id && (s.CreateUserId == user.Id || admin));

            if (item == null)
            {
                return new Result { Success = false, Message = "项目不存在" };
            }

            item.Description = model.Description;
            item.Name = model.Name;
            item.Contact = model.Contact;
            item.EndTime = model.EndTime;
            item.PositionType = model.PositionType;
            item.PositionTypeName = model.PositionTypeName;
            item.Price = model.Price;
            item.Type = model.Type;

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
            item.Images.AddRange(model.Images.Where(s => s.Id == 0).Select(s => new StallImage
            {
                Image = s.Image,
                Note = s.Note,
                Priority = s.Priority,
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
                }
            }
            item.Audios.AddRange(model.Audios.Where(s => s.Id == 0).Select(s => new StallAudio
            {
                BriefIntroduction = s.BriefIntroduction,
                Duration = s.Duration,
                Id = s.Id,
                Name = s.Name,
                Priority = s.Priority,
                Thumbnail = s.Thumbnail,
                Url = s.Url,
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
                }
            }
            item.Texts.AddRange(model.Texts.Where(s => s.Id == 0).Select(s => new StallText
            {
                Name = s.Name,
                Content = s.Content,
            }));

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _stallRepository.UpdateAsync(item);

            return new Result { Success = true, Message = model.Id.ToString() };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<StallOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Stall, long>(_stallRepository.GetAll().AsSingleQuery().Include(s => s.CreateUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.CreateUser.UserName.Contains(model.SearchText)));

            return new QueryResultModel<StallOverviewModel>
            {
                Items = await items.Select(s => new StallOverviewModel
                {
                    Id = s.Id,
                    CreateTime = s.CreateTime,
                    EndTime = s.EndTime,
                    Name = s.Name,
                    UserId = s.CreateUser.Id,
                    UserName = s.CreateUser.UserName,
                    Price = s.Price,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
