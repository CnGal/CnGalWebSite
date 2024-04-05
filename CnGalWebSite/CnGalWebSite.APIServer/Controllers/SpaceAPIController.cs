using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Users.Dtos;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.ExamineModel.Users;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/space/[action]")]
    public class SpaceAPIController : ControllerBase
    {
        
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<ApplicationUser, long> _userRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Message, int> _messageRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Video, long> _videoRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IExamineService _examineService;
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;
        private readonly IRankService _rankService;
        private readonly ISteamInforService _steamInforService;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<UserCertification, long> _userCertificationRepository;
        private readonly IEditRecordService _editRecordService;
        private readonly IQueryService _queryService;
        private readonly IRepository<UserIntegral, string> _userIntegralRepository;

        public SpaceAPIController(IRepository<Message, int> messageRepository, IMessageService messageService, IAppHelper appHelper, IRepository<ApplicationUser, long> userRepository, IRepository<Entry, int> entryRepository, IRepository<Video, long> videoRepository,
         IRepository<SignInDay, long> signInDayRepository, IRepository<Article, long> articleRepository, IUserService userService, IRepository<UserCertification, long> userCertificationRepository,
        IRepository<Examine, long> examineRepository, IExamineService examineService, IRankService rankService, IRepository<FavoriteObject, long> favoriteObjectRepository, IEditRecordService editRecordService,
        ISteamInforService steamInforService, IQueryService queryService, IRepository<UserIntegral, string> userIntegralRepository)
        {
            _examineRepository = examineRepository;
            _examineService = examineService;
            _appHelper = appHelper;
            
            _messageService = messageService;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _signInDayRepository = signInDayRepository;
            _articleRepository = articleRepository;
            _userService = userService;
            _rankService = rankService;
            _favoriteObjectRepository = favoriteObjectRepository;
            _steamInforService = steamInforService;
            _userCertificationRepository = userCertificationRepository;
            _editRecordService = editRecordService;
            _entryRepository = entryRepository;
            _videoRepository = videoRepository;
            _queryService = queryService;
            _userIntegralRepository = userIntegralRepository;
        }

        /// <summary>
        /// 通过Id获取用户的真实数据 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserInforViewModel>> GetUserDataAsync(string id)
        {
            //判断是否为当前登入用户
            var user_ = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var user = await _userRepository.GetAll().AsNoTracking().Where(s => s.Id == id)
                .Include(s => s.SignInDays)
                .Select(s => new ApplicationUser
                {
                    SignInDays = s.SignInDays,
                    Id = s.Id,
                    BackgroundImage = s.BackgroundImage,
                    PhotoPath = s.PhotoPath,
                    UserName = s.UserName,
                    PersonalSignature = s.PersonalSignature,
                    DisplayIntegral = s.DisplayIntegral,
                    SBgImage = s.SBgImage,
                    MBgImage=s.MBgImage,
                    GCoins=s.GCoins
                }).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            //判断是否为当前用户 是则加载待审核信息
            if (user_ != null && user_.Id == user.Id)
            {
                var examines = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && s.IsPassed == null)
               .Select(n => new Examine { Operation = n.Operation, Context = n.Context })
               .ToListAsync();

                var examine1 = examines.Find(s => s.Operation == Operation.UserMainPage && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
                examine1 = examines.Find(s => s.Operation == Operation.EditUserMain && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
            }
            return await _userService.GetUserInforViewModel(user);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [HttpGet]
        public async Task<ActionResult<PersonalSpaceViewModel>> GetUserViewAsync(string id = "")
        {
            ApplicationUser user = null;

            if (id == "")
            {
                //获取当前登入用户
                //获取当前用户ID
                user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
                id = user.Id;
            }

            //获取当前用户ID
            try
            {
                user = await _userRepository.GetAll().AsNoTracking()
                                          .Include(x => x.SignInDays)
                                          .Include(s => s.FileManager)
                                          .Include(x=>x.Certification).ThenInclude(s=>s.Entry)
                                          .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch
            {
                return NotFound();
            }

            if (user == null)
            {
                //未找到用户
                return NotFound();
            }

            //判断是否为当前登入用户
            var isCurrentUser = false;
            var user_ = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user_ != null && user_.Id == user.Id)
            {
                isCurrentUser = true;
            }

            //拉取审核数据
            var examines = await _examineRepository.GetAll().Where(s => s.ApplicationUserId == user.Id)
                .Select(n => new Examine { IsPassed = n.IsPassed, Operation = n.Operation, Context = n.Context, EntryId = n.EntryId, ArticleId = n.ArticleId, TagId = n.TagId, ApplyTime = n.ApplyTime, Id = n.Id })
                .ToListAsync();
            //如果是当前用户 则加载待审核的用户信息
            if (isCurrentUser == true)
            {
                var examine1 = examines.Find(s => s.Operation == Operation.UserMainPage && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
                examine1 = examines.Find(s => s.Operation == Operation.EditUserMain && s.IsPassed == null);
                if (examine1 != null)
                {
                    await _userService.UpdateUserData(user, examine1);
                }
            }

            var userEditInfor = await _userService.GetUserEditInforBindModel(user);

            var model = new PersonalSpaceViewModel
            {
                MainPageContext = user.MainPageContext,
                Id = user.Id,
                IsCurrentUser = isCurrentUser,
                ContributionValue = user.DisplayContributionValue,
                OnlineTime = ((double)user.OnlineTime / 60 / 60),
                LastOnlineTime = user.LastOnlineTime,
                CanComment = user.CanComment ?? true,
                RegisteTime = user.RegistTime,
                Birthday = user.Birthday,
                IsShowFavorites = user.IsShowFavotites,
                PassedExamineCount = userEditInfor.PassedExamineCount,
                UnpassedExamineCount = userEditInfor.UnpassedExamineCount,
                PassingExamineCount = userEditInfor.PassingExamineCount,
                UsedFilesSpace = userEditInfor.UsedFilesSpace,
                TotalFilesSpace = userEditInfor.TotalFilesSpace,
                TotalExamine = userEditInfor.EditCount,
                LastEditTime = userEditInfor.LastEditTime,
                SteamId = user.SteamId,
                IsShowGameRecord = user.IsShowGameRecord,
                BasicInfor = await _userService.GetUserInforViewModel(user),
                UserCertification = user.Certification?.Entry != null ?_appHelper.GetEntryInforTipViewModel(user.Certification.Entry) : null

            };

            //提前将MarkDown语法转为Html
            model.MainPageContext = _appHelper.MarkdownToHtml(model.MainPageContext);

            //计算各个部分编辑数目
            model.EditEntryNum = examines.Count(s => (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishMainPage));
            model.CreateArticleNum = _articleRepository.Count(s => s.CreateUserId == user.Id);

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<PagedResultDto<ExaminedNormalListModel>> GetUserEditRecordAsync([FromQuery]string userId, [FromQuery]string sorting, [FromQuery]int maxResultCount, [FromQuery] int currentPage, [FromQuery]string screeningConditions)
        {
            return await _examineService.GetPaginatedResult(new GetExamineInput
            {
                ScreeningConditions = screeningConditions,
                Sorting = sorting,
                CurrentPage = currentPage,
                MaxResultCount = maxResultCount,
                UserId = userId
            }, 0, userId);
        }

        [HttpGet]
        public async Task<ActionResult<List<Message>>> GetUserMessagesAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var messages = await _messageRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == user.Id).AsNoTracking().ToListAsync();

            foreach (var item in messages)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "user.png");
            }

            return messages;
        }

        [HttpGet("{currentPage}/{MaxResultCount}/{IsVisual}")]
        public async Task<ActionResult<PagedResultDto<Message>>> GetUserMessageAsync(int currentPage, int MaxResultCount, bool IsVisual)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var input = new GetMessageInput
            {
                CurrentPage = currentPage,
                MaxResultCount = MaxResultCount,
                ScreeningConditions = "全部",
                IsVisual = IsVisual
            };
            var dtos = await _messageService.GetPaginatedResult(input, user.Id);

            //需要清除环回引用
            foreach (var item in dtos.Data)
            {
                if (item.ApplicationUser != null)
                {
                    item.ApplicationUser = null;
                }
                //获取图片链接
                item.Image = _appHelper.GetImagePath(item.Image, "user.png");
            }
            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<Result>> SignInAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //转换成东八区
            var dateTime = DateTime.Now.ToCstTime();//.AddHours(8);
            //计算连续签到天数和今天是否签到
            if (await _signInDayRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.Time.Date == dateTime.Date) == false)
            {
                await _signInDayRepository.InsertAsync(new SignInDay
                {
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    Time = DateTime.Now.ToCstTime()
                });
            }

            //更新用户积分
            await _userService.UpdateUserIntegral(user);
            await _rankService.UpdateUserRanks(user);

            return new Result { Successful = true };

        }

        [HttpGet]
        public async Task<ActionResult<EditUserMainPageViewModel>> EditMainPageAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //判断用户是否有待审核的主页编辑记录
            var examine = await _examineService.GetUserInforActiveExamineAsync(user.Id, Operation.UserMainPage);

            if (examine != null)
            {
                await _userService.UpdateUserData(user, examine);
            }
            var model = new EditUserMainPageViewModel
            {
                Id = user.Id,
                MainPage = user.MainPageContext
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditMainPageAsync(EditUserMainPageViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查是否超过编辑上限
            if (await _examineRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == null) > ToolHelper.MaxEditorCount)
            {
                return new Result { Successful = false, Error = "当前已超过最大待审核编辑数目，请等待审核通过后继续编辑，长时间未更新请联系管理员" };
            }

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(user, user, model.MainPage, Operation.UserMainPage, model.Note);


            return new Result { Successful = true };

        }

        [HttpGet]
        public async Task<ActionResult<EditUserDataViewModel>> EditUserDataAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            user = await _userRepository.GetAll()
                .Include(s => s.ThirdPartyLoginInfors)
                .Include(s => s.Certification).ThenInclude(s => s.Entry)
                .FirstOrDefaultAsync(s => s.Id == user.Id);

            //判断用户是否有待审核的主页编辑记录
            var examine = await _examineService.GetUserInforActiveExamineAsync(user.Id, Operation.EditUserMain);
            if (examine != null)
            {
                await _userService.UpdateUserData(user, examine);
            }


            EditUserDataViewModel model = new();
            model.Email = ToolHelper.GetxxxString(user.Email);
            model.Phone = ToolHelper.GetxxxString(user.PhoneNumber);
            model.UserName = user.UserName;
            model.BackgroundName = user.BackgroundImage;
            model.MBgImageName = user.MBgImage;
            model.SBgImageName = user.SBgImage;
            model.PhotoName = user.PhotoPath;
            model.Birthday = user.Birthday;
            model.PersonalSignature = user.PersonalSignature;
            model.CanComment = user.CanComment ?? true;
            model.SteamIds = user.SteamId?.Split(",")?.Where(s=>!string.IsNullOrWhiteSpace(s))?.ToList()??[];
            model.Id = user.Id;
            model.IsShowFavorites = user.IsShowFavotites;
            model.IsShowGameRecord = user.IsShowGameRecord;
            model.QQAccountName = user.ThirdPartyLoginInfors.FirstOrDefault(s => s.Type == ThirdPartyLoginType.QQ)?.Name;
            model.GroupQQ = user.GroupQQ == 0 ? "" : ToolHelper.GetxxxString(user.GroupQQ.ToString());
            model.LastChangePasswordTime = user.LastChangePasswordTime;
            var temp = user.ThirdPartyLoginInfors.FirstOrDefault(s => s.Type == ThirdPartyLoginType.QQ);
            model.Ranks = await _rankService.GetUserRankListForEdit(user);


            //判断用户是否有认证申请
            examine = await _examineService.GetUserInforActiveExamineAsync(user.Id, Operation.RequestUserCertification);
            if (examine != null)
            {
                var userCertification = new UserCertification();
               await  _userService.UpdateUserCertificationData(userCertification, examine);

                model.UserCertificationModel = new EditUserCertificationModel
                {
                    IsPending = true,
                    EntryName = userCertification.Entry.DisplayName,
                    Note = examine.Note,
                    Type = userCertification.Entry.Type == EntryType.ProductionGroup? UserCertificationType.Group: UserCertificationType.Staff,
                };
            }
            else
            {
                model.UserCertificationModel = new EditUserCertificationModel
                {
                    EntryName = user.Certification?.Entry?.DisplayName,
                    Type = (user.Certification?.Entry?.Type ?? EntryType.Staff) == EntryType.ProductionGroup ? UserCertificationType.Group : UserCertificationType.Staff
                };
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditUserDataAsync(EditUserDataViewModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //检查重名
            if (user.UserName != model.UserName)
            {
                if (await _userRepository.GetAll().AnyAsync(s => s.UserName == model.UserName))
                {
                    return new Result { Successful = false, Error = "该用户名已被使用" };
                }
            }


            user.Birthday = model.Birthday;
            user.CanComment = model.CanComment;

            //判断SteamId是否改变
            var steamId = string.Join(",", model.SteamIds);
            if (steamId != user.SteamId)
            {
                // 判断SteamId是否已经被绑定
                foreach(var item in model.SteamIds)
                {
                    if(await _userRepository.AnyAsync(s=>s.SteamId.Contains(item)))
                    {
                        return new Result { Successful = false, Error = $"SteamId被其他用户绑定：{item}" };
                    }
                }


                user.SteamId = steamId;
                user = await _userRepository.UpdateAsync(user);
                if (string.IsNullOrWhiteSpace(steamId) == false)
                {
                    //更新游戏信息
                    if (await _steamInforService.UpdateUserSteam(user) == false)
                    {
                        return new Result { Successful = false, Error = "无法获取Steam信息，请检查SteamId是否正确；也可能是服务器网络波动，不填写该项以保存其他修改的内容" };
                    }
                    //尝试添加G币
                    await _userService.TryAddGCoins(user.Id, UserIntegralSourceType.BindSteamId, 10, null);
                }
            }
            user.IsShowGameRecord = model.IsShowGameRecord;
            //user.IsShowFavotites = model.IsShowFavorites;
            //更新头衔是否显示
            await _rankService.UpdateUserRanksIsHidden(user, model.Ranks);

            await _userRepository.UpdateAsync(user);


            //敏感数据 判断是否修改
            if (user.UserName != model.UserName || user.PersonalSignature != model.PersonalSignature
                || user.PhotoPath != model.PhotoName || user.BackgroundImage != model.BackgroundName
                || user.MBgImage != model.MBgImageName || user.SBgImage != model.SBgImageName)
            {
                //添加修改记录
                //新建审核数据对象
                var userMain = new UserMain
                {
                    UserName = model.UserName,
                    PersonalSignature = model.PersonalSignature,
                    PhotoPath = model.PhotoName,
                    BackgroundImage = model.BackgroundName,
                    MBgImage = model.MBgImageName,
                    SBgImage = model.SBgImageName
                };

                //保存并尝试应用审核记录
                await _editRecordService.SaveAndApplyEditRecord(user, user, userMain, Operation.EditUserMain, model.Note);
            }

            return new Result { Successful = true };
        }

        [HttpGet]
        public async Task<ActionResult<Result>> ReadedAllMessagesAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            await _messageRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsReaded, b => true));

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditMessageIsReadedAsync(EditMessageIsReadedModel model)
        {
            await _messageRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsReaded, b => model.IsReaded));

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> DeleteMessagesAsync(DeleteMessagesModel model)
        {
            await _messageRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteDeleteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserEditMessageIsReadedAsync(EditMessageIsReadedModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = _userService.CheckCurrentUserRole( "Admin");
            await _messageRepository.GetAll().Where(s => (true || s.ApplicationUserId == user.Id) && model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsReaded, b => model.IsReaded));

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<MessageOverviewModel>> ListMessages(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Message, int>(_messageRepository.GetAll().AsSingleQuery().Include(s=>s.ApplicationUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Text.Contains(model.SearchText) || s.Title.Contains(model.SearchText) || s.LinkTitle.Contains(model.SearchText)));

            return new QueryResultModel<MessageOverviewModel>
            {
                Items = await items.Select(s => new MessageOverviewModel
                {
                    Id = s.Id,
                    Type = s.Type,
                    PostTime = s.PostTime,
                    Text = s.Text,
                    Title = s.Title,
                    UserId = s.ApplicationUser.Id,
                    UserName=s.ApplicationUser.UserName,
                    Link = s.Link,
                    IsReaded = s.IsReaded
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserDeleteMessagesAsync(DeleteMessagesModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //判断是否为管理员
            var isAdmin = _userService.CheckCurrentUserRole( "Admin");
            await _messageRepository.GetAll().Where(s => (isAdmin || s.ApplicationUserId == user.Id) && model.Ids.Contains(s.Id)).ExecuteDeleteAsync();

            return new Result { Successful = true };
        }

        [HttpGet]
        public async Task<ActionResult<long>> GetUserUnReadedMessageCountAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            return await _messageRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.IsReaded == false);
        }

        /// <summary>
        /// 获取每日任务
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserTaskModel>> GetUserTasks()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            user = await _userRepository.GetAll().Include(s => s.SignInDays).FirstOrDefaultAsync(s => s.Id == user.Id);

            var dateTime = DateTime.Now.ToCstTime();
            var model = new UserTaskModel
            {
                IsSignIn = await _signInDayRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.Time.Date == dateTime.Date),
                IsComment = await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == true && s.ApplyTime.Date == dateTime.Date && s.Operation == Operation.PubulishComment),
                IsEdit = await _examineRepository.GetAll().AnyAsync(s => s.ApplicationUserId == user.Id && s.IsPassed == true && s.ApplyTime.Date == dateTime.Date && s.Operation != Operation.PubulishComment),
            };

            //获取签到信息
            var sign =_userService.GetUserSignInDays(user);
            model.IsSignIn = sign.IsSignIn;
            model.SignInDays = sign.SignInDays;

            //绑定SteamId
            if (await _userIntegralRepository.AnyAsync(s=>s.ApplicationUserId==user.Id))
            {
                model.IsBindSteamId = true;
            }
           else
            {
                if (string.IsNullOrWhiteSpace(user.SteamId))
                {
                    model.IsBindSteamId = false;
                }
                else
                {
                    await _userService.TryAddGCoins(user.Id, UserIntegralSourceType.BindSteamId, 10, null);
                    model.IsBindSteamId = true;
                }
            }

            return model;

        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserEditInforBindModel>> GetUserEditInforBindModelAsync(string id)
        {

            var user = await _userRepository.GetAll().Include(s => s.FileManager).FirstOrDefaultAsync(s => s.Id == id);
            if (user == null)
            {
                return NotFound("无法找到该用户");
            }

            return await _userService.GetUserEditInforBindModel(user);
        }

        /// <summary>
        /// 编辑个人收件地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<EditUserAddressModel>> EditUserAddress()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            user = await _userRepository.GetAll().AsNoTracking()
                .Include(s => s.UserAddress)
                .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (user.UserAddress == null)
            {
                return new EditUserAddressModel();
            }


            var model = new EditUserAddressModel
            {
                Address = user.UserAddress.Address,
                PhoneNumber = user.UserAddress.PhoneNumber,
                RealName = user.UserAddress.RealName
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditUserAddress(EditUserAddressModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            _userRepository.Clear();
            user = await _userRepository.GetAll().AsNoTracking()
                .Include(s => s.UserAddress)
                .FirstOrDefaultAsync(s => s.Id == user.Id);
            if (user.UserAddress == null)
            {
                user.UserAddress = new UserAddress();
            }
            user.UserAddress.Address = model.Address;
            user.UserAddress.RealName = model.RealName;
            user.UserAddress.PhoneNumber = model.PhoneNumber;

            await _userRepository.UpdateAsync(user);

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> AddUserIntegralAsync(AddUserIntegralModel model)
        {
            if (await _userRepository.GetAll().AnyAsync(s => s.Id == model.UserId) == false)
            {
                return new Result { Successful = false, Error = "未找到该用户" };
            }

            await _userService.AddUserIntegral(model);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 解除绑定群聊QQ号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> UnBindGroupQQAsync(UnBindGroupQQModel model)
        {
            //提前判断是否通过人机验证
            if (_appHelper.CheckRecaptcha(model.Verification) == false)
            {
                return BadRequest(new Result { Error = "没有通过人机验证" });
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if(user == null)
            {
                return new Result { Error = "未找到该用户" };
            }

            user.GroupQQ = 0;
            await _userRepository.UpdateAsync(user);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 生成绑定QQ的身份识别码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> GetBindGroupQQCodeAsync(BindGroupQQModel model)
        {
            //提前判断是否通过人机验证
            if (_appHelper.CheckRecaptcha(model.Verification) == false)
            {
                return BadRequest(new Result { Error = "没有通过人机验证" });
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                return new Result { Error = "未找到该用户" };
            }


            return new Result { Successful = true, Error = await _userService.GenerateBindGroupQQCode(user) };
        }

        /// <summary>
        /// 获取用户发表的文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserArticleListModel>> GetUserArticles(string id)
        {
            var items = await _articleRepository.GetAll().Include(s=>s.CreateUser).AsNoTracking().Where(s => s.CreateUserId == id&&s.IsHidden==false&&string.IsNullOrWhiteSpace(s.Name)==false).ToListAsync();

            var model =new UserArticleListModel();
            foreach (var item in items.OrderByDescending(s=>s.Id))
            {
                model.Items.Add(_appHelper.GetArticleInforTipViewModel(item));
            }

            return model;
        }

        /// <summary>
        /// 获取用户发表的视频
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserVideoListModel>> GetUserVideos(string id)
        {
            var items = await _videoRepository.GetAll().Include(s => s.CreateUser).AsNoTracking().Where(s => s.CreateUserId == id && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).ToListAsync();

            var model = new UserVideoListModel();
            foreach (var item in items.OrderByDescending(s => s.Id))
            {
                model.Items.Add(_appHelper.GetVideoInforTipViewModel(item));
            }

            return model;
        }

        /// <summary>
        /// 获取用户待审核的编辑对象列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<UserPendingDataModel>>> GetUserPendingData()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            return await _examineService.GetUserPendingData(user.Id);
        }

        /// <summary>
        /// 编辑用户认证
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> EditUserCertificationAsync(EditUserCertificationModel model)
        {
            //提前判断是否通过人机验证
            //if (_appHelper.CheckRecaptcha(model.Verification) == false)
            //{
            //    return BadRequest(new Result { Error = "没有通过人机验证" });
            //}
            Entry entry = null;

            if(string.IsNullOrWhiteSpace(model.EntryName)==false)
            {
                if ((await _userService.GetAllNotCertificatedEntriesAsync()).Contains(model.EntryName)==false)
                {
                    return new Result { Successful = false, Error = "词条不存在或被占用" };
                }
                entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Name == model.EntryName);

                if(entry==null)
                {
                    return new Result { Successful = false, Error = "词条不存在或被占用" };
                }
            }

            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var userCertification = await _userCertificationRepository.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

            var userCertificationMain = new UserCertificationMain();

            if (userCertification == null)
            {
                if (entry==null)
                {
                    await _examineRepository.DeleteAsync(s => s.ApplicationUserId == user.Id && s.Operation == Operation.RequestUserCertification && s.IsPassed == null);
                    return new Result { Successful = true };
                }
                else
                {
                    userCertification = await _userCertificationRepository.InsertAsync(new UserCertification
                    {
                        ApplicationUserId = user.Id
                    });

                    userCertificationMain.EntryId = entry.Id;


                }

            }
            else
            {
                if (entry == null)
                {
                    userCertification.EntryId = null;
                    userCertification.Entry = null;

                    userCertification= await _userCertificationRepository.UpdateAsync(userCertification);

                    await _examineRepository.DeleteAsync(s => s.ApplicationUserId == user.Id && s.Operation == Operation.RequestUserCertification && s.IsPassed == null);
                    return new Result { Successful = true };
                }
                else
                {
                    userCertificationMain.EntryId = entry.Id;

                }
            }

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(userCertification, user, userCertificationMain, Operation.RequestUserCertification, model.Note, false, false);

            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取输入提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> GetAllNotCertificatedEntriesAsync([FromQuery] EntryType type)
        {
            return await _userService.GetAllNotCertificatedEntriesAsync(type);
        }


        /// <summary>
        /// 获取用户热力图
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<EChartsHeatMapOptionModel>> GetUserHeatMap([FromQuery]string id, [FromQuery]UserHeatMapType type, [FromQuery] long afterTime, [FromQuery] long beforeTime)
        {
            var after = afterTime.ToString().TransTime();
            var before = beforeTime.ToString().TransTime();

            return type switch
            {
                UserHeatMapType.EditRecords => await _userService.GetUserEditRecordHeatMap(id, after, before),
                UserHeatMapType.SignInDays => await _userService.GetUserSignInDaysHeatMap(id, after, before)
            };
        }

    }
}
