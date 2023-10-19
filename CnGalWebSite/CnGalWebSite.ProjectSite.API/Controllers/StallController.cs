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
using CnGalWebSite.ProjectSite.Models.ViewModels.Share;
using CnGalWebSite.ProjectSite.API.Services.Stalls;
using CnGalWebSite.ProjectSite.API.Services.Notices;
using CnGalWebSite.ProjectSite.Models.ViewModels.Messages;
using CnGalWebSite.ProjectSite.API.Services.Messages;


namespace CnGalWebSite.ProjectSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StallController : ControllerBase
    {
        private readonly IRepository<Stall, long> _stallRepository;
        private readonly IRepository<StallInformationType, long> _stallInformationTypeRepository;
        private readonly IRepository<StallUser, long> _stallUserRepository;
        private readonly IUserService _userService;
        private readonly IStallService _stallService;
        private readonly IQueryService _queryService;
        private readonly INoticeService _noticeService;
        private readonly IMessageService _messageService;

        public StallController(IRepository<Stall, long> stallRepository, IUserService userService, IQueryService queryService, IStallService stallService, IRepository<StallInformationType, long> stallInformationTypeRepository, INoticeService noticeService,
            IRepository<StallUser, long> stallUserRepository, IMessageService messageService)
        {
            _stallRepository = stallRepository;
            _userService = userService;
            _queryService = queryService;
            _stallService = stallService;
            _stallInformationTypeRepository = stallInformationTypeRepository;
            _noticeService = noticeService;
            _stallUserRepository = stallUserRepository;
            _messageService = messageService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<StallViewModel>> GetAsync([FromQuery] long id)
        {
            var user = await _userService.GetCurrentUserAsync();

            var item = await _stallRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Audios)
                .Include(s => s.Informations).ThenInclude(s => s.Type)
                .Include(s => s.Texts)
                .Include(s=>s.Users).ThenInclude(s=>s.User)
                .FirstOrDefaultAsync(s => s.Id == id && s.Hide == false);

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
                Contact = item.Users.Any(s => s.UserId == user?.Id && s.Passed == true) ? item.Contact : null,
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
                    Content = s.Content,
                    Link = s.Link,
                }).ToList(),
                Informations = item.Informations.Where(s => s.Type != null && s.Type.Hide == false).OrderByDescending(s => s.Type.Priority).Select(s => new StallInformationViewModel
                {
                    Icon = s.Type.Icon,
                    Name = s.Type.Name,
                    Value = s.Value,
                    Priority = s.Type.Priority
                }).ToList(),
                CreateUser = await _userService.GetUserInfo(item.CreateUserId),
                Users = item.Users.Select(s => new StallUserViewModel
                {
                    User = _userService.GetUserInfo(s.User),
                    Id = s.Id,
                    Passed = s.Passed,
                    Contact = item.CreateUserId == user?.Id && s.Passed == true ? s.User.Contact : null
                }).ToList()
            };

            return model;
        }


        [HttpGet]
        public async Task<ActionResult<StallEditModel>> EditAsync([FromQuery] long id)
        {
            var informationTypes = await _stallInformationTypeRepository.GetAll().Where(s => s.Hide == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            if (id == 0)
            {
                return new StallEditModel
                {
                    EndTime = DateTime.Now.ToCstTime().AddDays(60),
                    Informations = informationTypes.Select(s => new StallInformationEditModel
                    {
                        Description = s.Description,
                        Name = s.Name,
                        Icon = s.Icon,
                        TypeId = s.Id,
                        Types = s.Types.ToList(),
                    }).ToList()
                };
            }

            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            var item = await _stallRepository.GetAll()
                .Include(s => s.Images)
                .Include(s => s.Audios)
                .Include(s => s.Informations)
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
                    Link = s.Link,
                    Id = s.Id
                }).ToList(),
            };

            foreach (var info in informationTypes)
            {
                var temp = item.Informations.FirstOrDefault(s => s.TypeId == info.Id);

                model.Informations.Add(new StallInformationEditModel
                {
                    Name = info.Name,
                    Value = temp?.Value,
                    Id = temp?.Id ?? 0,
                    Description = info.Description,
                    Icon = info.Icon,
                    TypeId = info.Id,
                    Types = info.Types.ToList()
                });
            }


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
                .Include(s => s.Informations)
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
                    info.Link = temp.Link;
                }
            }
            item.Texts.AddRange(model.Texts.Where(s => s.Id == 0).Select(s => new StallText
            {
                Name = s.Name,
                Content = s.Content,
                Link = s.Link,
            }));
            //附加信息
            model.Informations.RemoveAll(s => string.IsNullOrWhiteSpace(s.Value));
            item.Informations.RemoveAll(s => model.Informations.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Informations)
            {
                var temp = model.Informations.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.TypeId = temp.TypeId;
                    info.Value = temp.Value;
                }
            }
            item.Informations.AddRange(model.Informations.Where(s => s.Id == 0).Select(s => new StallInformation
            {
                TypeId = s.TypeId,
                Value = s.Value,
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
                    UserId = s.CreateUserId,
                    UserName = s.CreateUser.UserName,
                    Price = s.Price,
                    Hide = s.Hide,
                    Priority = s.Priority,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> HideAsync(HideModel model)
        {
            var user = await _userService.GetCurrentUserAsync();
            var admin = _userService.CheckCurrentUserRole("Admin");

            await _stallRepository.GetAll().Where(s => model.Id == s.Id && (user.Id == s.CreateUserId || admin)).ExecuteUpdateAsync(s => s.SetProperty(s => s.Hide, b => model.Hide));
            return new Result { Success = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditPriorityAsync(EditPriorityModel model)
        {
            var stall = await _stallRepository.GetAll().AsNoTracking()
                                .Include(s => s.CreateUser)
                                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (stall == null)
            {
                return new Result { Success = false, Message = "找不到目标" };
            }
            if (stall.Priority <= 0 && model.PlusPriority > 0 && stall.Hide == false)
            {
                //通知
                _noticeService.PutNotice($"【橱窗上新】\n“{stall.CreateUser.GetName()}”发布了“{stall.Name}”橱窗\nhttps://www.cngal.org.cn/stall/{model.Id}");
            }

            await _stallRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<List<StallInfoViewModel>> GetAll()
        {
            var now = DateTime.Now.ToCstTime();
            var projects = await _stallRepository.GetAll()
                .Where(s => s.Priority > 0 && s.Hide == false && s.EndTime > now)
                .Include(s => s.Images)
                .Include(s => s.CreateUser)
                .Include(s => s.Informations).ThenInclude(s => s.Type)
                .OrderByDescending(s => s.Priority)
                .ToListAsync();

            return projects.Select(s => _stallService.GetStallInfoViewModel(s)).ToList();
        }

        /// <summary>
        /// 列出所有词条基础信息类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<StallInformationTypeOverviewModel>> ListEntryInformationTypes(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<StallInformationType, long>(_stallInformationTypeRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.Description.Contains(model.SearchText)));

            return new QueryResultModel<StallInformationTypeOverviewModel>
            {
                Items = await items.Select(s => new StallInformationTypeOverviewModel
                {
                    Icon = s.Icon,
                    Hide = s.Hide,
                    Name = s.Name,
                    Types = s.Types.ToList(),
                    Description = s.Description,
                    Id = s.Id,
                    HideInfoCard = s.HideInfoCard,
                    Priority = s.Priority,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        /// <summary>
        /// 获取词条基础信息类型编辑模型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StallInformationTypeEditModel>> EditStallInformationType(long id)
        {
            var item = await _stallInformationTypeRepository.GetAll()
              .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("找不到目标");
            }

            var model = new StallInformationTypeEditModel
            {
                Icon = item.Icon,
                Hide = item.Hide,
                Name = item.Name,
                Types = item.Types.ToList(),
                Description = item.Description,
                Id = item.Id,
                HideInfoCard = item.HideInfoCard,
            };

            return model;
        }

        /// <summary>
        /// 编辑词条基础信息类型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<Result> EditStallInformationType(StallInformationTypeEditModel model)
        {

            StallInformationType item = null;
            if (model.Id == 0)
            {
                item = await _stallInformationTypeRepository.InsertAsync(new StallInformationType
                {
                    Icon = model.Icon,
                    Hide = model.Hide,
                    Name = model.Name,
                    HideInfoCard = model.HideInfoCard,
                    Types = model.Types.ToArray(),
                    Description = model.Description,
                });
                model.Id = item.Id;
                _stallInformationTypeRepository.Clear();
            }

            item = await _stallInformationTypeRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Success = false, Message = "目标不存在" };
            }



            //基本信息

            item.Name = model.Name;
            item.Icon = model.Icon;
            item.Description = model.Description;
            item.Hide = model.Hide;
            item.HideInfoCard = model.HideInfoCard;
            item.Types = model.Types.ToArray();

            //保存
            await _stallInformationTypeRepository.UpdateAsync(item);


            return new Result { Success = true };
        }

        /// <summary>
        /// 获取对应类型词条的基础信息类型列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<List<StallInformationTypeViewModel>> GetStallInformationTypes(int type)
        {
            return (await _stallInformationTypeRepository.GetAll()
                             .Where(s => s.Hide == false && string.IsNullOrWhiteSpace(s.Name) == false)
                             .ToListAsync())
                             .Where(s => s.Types.Contains((ProjectPositionType)type))
                             .Select(s => new StallInformationTypeViewModel
                             {
                                 Description = s.Description,
                                 Icon = s.Icon,
                                 Name = s.Name,
                             }).ToList();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditStallInformationTypePriorityAsync(EditPriorityModel model)
        {
            await _stallInformationTypeRepository.GetAll().Where(s => model.Id == s.Id).ExecuteUpdateAsync(s => s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<Result> ApplyStall(ApplyStallModel model)
        {
            var user = await _userService.GetCurrentUserAsync();

            var stall = await _stallRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.StallId);
            if (stall == null)
            {
                return new Result { Success = false, Message = "找不到这个橱窗" };
            }

            if (stall.EndTime < DateTime.Now.ToCstTime())
            {
                return new Result { Success = false, Message = "已经过了截止日期" };
            }

            if (await _stallUserRepository.AnyAsync(s => s.StallId == model.StallId && s.UserId == user.Id))
            {
                if (model.Apply)
                {
                    return new Result { Success = false, Message = "你已经邀请了" };
                }
                else
                {
                    await _stallUserRepository.DeleteAsync(s => s.StallId == model.StallId && s.UserId == user.Id);

                    //向用户发送消息
                    await _messageService.PutMessage(new PutMessageModel
                    {
                        PageId = stall.Id,
                        PageType = PageType.Stall,
                        Text = $"“{user.GetName()}”取消了“{stall.Name}”约稿请求",
                        Type = MessageType.ApplyStall,
                        UserId = stall.CreateUserId,
                    });

                    return new Result { Success = true };
                }
            }

            await _stallUserRepository.InsertAsync(new StallUser
            {
                StallId = model.StallId,
                UserId = user.Id,
            });

            //向用户发送消息
            await _messageService.PutMessage(new PutMessageModel
            {
                PageId = stall.Id,
                PageType = PageType.Stall,
                Text = $"“{user.GetName()}”请求“{stall.Name}”的约稿",
                Type = MessageType.ApplyStall,
                UserId = stall.CreateUserId,
            });

            return new Result { Success = true };
        }

        [HttpPost]
        public async Task<Result> ProcStallUser(ProcStallApplicationModel model)
        {
            var user = await _userService.GetCurrentUserAsync();

            var positionUser = await _stallUserRepository.GetAll()
                .Include(s => s.Stall)
                .FirstOrDefaultAsync(s => s.Id == model.UserId);

            if (positionUser == null)
            {
                return new Result { Success = false, Message = "找不到这个用户" };
            }

            if (positionUser.Stall.CreateUserId != user.Id && !_userService.CheckCurrentUserRole("Admin"))
            {
                return new Result { Success = false, Message = "权限不足" };
            }

            positionUser.Passed = model.Passed;

            await _stallUserRepository.UpdateAsync(positionUser);

            //向用户发送消息
            await _messageService.PutMessage(new PutMessageModel
            {
                PageId = positionUser.Stall.Id,
                PageType = PageType.Stall,
                Text = $"“{positionUser.Stall.Name}”的创作者{(model.Passed == true ? "接受" : "拒绝")}了你的约稿请求",
                Type = MessageType.PassedStallUser,
                UserId = positionUser.UserId,
            });


            return new Result { Success = true };
        }
    }
}
