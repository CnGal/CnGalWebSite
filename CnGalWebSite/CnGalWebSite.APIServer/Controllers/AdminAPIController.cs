using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.ElasticSearches;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.HistoryData;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Votes;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Home;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/admin/[action]")]
    public class AdminAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<FriendLink, int> _friendLinkRepository;
        private readonly IRepository<Carousel, int> _carouselRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<Disambig, int> _disambigRepository;
        private readonly IRepository<Message, long> _messageRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<ThumbsUp, long> _thumbsUpRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<UserOnlineInfor, long> _userOnlineInforRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<ErrorCount, long> _errorCountRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<FavoriteObject, long> _favoriteObjectRepository;
        private readonly IRepository<Rank, long> _rankRepository;
        private readonly IRepository<Periphery, long> _peripheryRepository;
        private readonly IRepository<Vote, long> _voteRepository;
        private readonly IRepository<SteamInfor, long> _steamInforRepository;
        private readonly IUserService _userService;
        private readonly IAppHelper _appHelper;
        private readonly IExamineService _examineService;
        private readonly IEntryService _entryService;
        private readonly IArticleService _articleService;
        private readonly ICommentService _commentService;
        private readonly IMessageService _messageService;
        private readonly IFileService _fileService;
        private readonly IErrorCountService _errorCountService;
        private readonly IFavoriteFolderService _favoriteFolderService;
        private readonly IRankService _rankService;
        private readonly IPeripheryService _peripheryService;
        private readonly IVoteService _voteService;
        private readonly IElasticsearchBaseService<Entry> _entryElasticsearchBaseService;
        private readonly IElasticsearchBaseService<Article> _articleElasticsearchBaseService;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly INewsService _newsService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IHistoryDataService _historyDataService;
        private readonly IConfiguration _configuration;


        public AdminAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRankService rankService, IHistoryDataService historyDataService,
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IElasticsearchService elasticsearchService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository, IPerfectionService perfectionService, IElasticsearchBaseService<Article> articleElasticsearchBaseService,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, IElasticsearchBaseService<Entry> entryElasticsearchBaseService,
        IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService, IRepository<Rank, long> rankRepository, INewsService newsService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository,
        IVoteService voteService, IRepository<Vote, long> voteRepository, IRepository<SteamInfor, long> steamInforRepository,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _tagRepository = tagRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _examineService = examineService;
            _roleManager = roleManager;
            _userService = userService;
            _entryService = entryService;
            _articleService = articleService;
            _friendLinkRepository = friendLinkRepository;
            _carouselRepository = carouselRepositor;
            _messageRepository = messageRepository;
            _commentRepository = commentRepository;
            _commentService = commentService;
            _messageService = messageService;
            _userRepository = userRepository;
            _userFileRepository = userFileRepository;
            _userOnlineInforRepository = userOnlineInforRepository;
            _thumbsUpRepository = thumbsUpRepository;
            _disambigRepository = disambigRepository;
            _fileService = fileService;
            _signInDayRepository = signInDayRepository;
            _errorCountService = errorCountService;
            _errorCountRepository = errorCountRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _favoriteFolderService = favoriteFolderService;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _favoriteObjectRepository = favoriteObjectRepository;
            _rankRepository = rankRepository;
            _rankService = rankService;
            _peripheryRepository = peripheryRepository;
            _peripheryService = peripheryService;
            _entryElasticsearchBaseService = entryElasticsearchBaseService;
            _articleElasticsearchBaseService = articleElasticsearchBaseService;
            _elasticsearchService = elasticsearchService;
            _newsService = newsService;
            _gameNewsRepository = gameNewsRepository;
            _weeklyNewsRepository = weeklyNewsRepository;
            _historyDataService = historyDataService;
            _voteService = voteService;
            _voteRepository = voteRepository;
            _configuration = configuration;
            _steamInforRepository = steamInforRepository;
        }

        /// <summary>
        /// 获取用户列表概览信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ListUsersInforViewModel>> ListUsersAsync()
        {
            var model = new ListUsersInforViewModel
            {
                UserCount = await _userManager.Users.CountAsync()
            };
            return model;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="input">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListUserAloneModel>>> GetUserListAsync(UsersPagesInfor input)
        {
            var dtos = await _userService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        /// <summary>
        /// 获取编辑用户视图模型 管理员模式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<EditUserViewModel>> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userClaims = await _userManager.GetClaimsAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PersonalSignature = user.PersonalSignature,
                MainPageContext = user.MainPageContext,
                Integral = user.Integral,
                ContributionValue = user.ContributionValue,
                Claims = userClaims.Select(c => c.Value).ToList(),
                MBgImageName = _appHelper.GetImagePath(user.MBgImage, ""),
                SBgImageName = _appHelper.GetImagePath(user.SBgImage, ""),
                PhotoName = _appHelper.GetImagePath(user.PhotoPath, ""),
                BackgroundName = _appHelper.GetImagePath(user.BackgroundImage, ""),
                CanComment = user.CanComment ?? true,
                Roles = new List<UserRolesModel>(),
                Birthday = user.Birthday,
                IsShowFavotites = user.IsShowFavotites,
                IsPassedVerification = user.IsPassedVerification
            };
            //获取用户角色
            var allRoles = _roleManager.Roles;
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var item in allRoles)
            {
                var isSelected = false;
                foreach (var infor in userRoles)
                {
                    if (item.Name == infor)
                    {
                        isSelected = true;
                        break;
                    }
                }
                model.Roles.Add(new UserRolesModel { Name = item.Name, IsSelected = isSelected });
            }

            return model;
        }

        /// <summary>
        /// 编辑用户 管理员模式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> EditUser(EditUserViewModel model)
        {
            //获取当前用户ID
            var user_admin = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return new Result { Error = "当前用户不存在，可能在编辑过程中被删除", Successful = false };
            }

            if (await _userManager.IsInRoleAsync(user, "SuperAdmin") && await _userManager.IsInRoleAsync(user_admin, "SuperAdmin") == false)
            {
                return new Result { Error = "你没有权限修改超级管理员的信息", Successful = false };
            }

            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PersonalSignature = model.PersonalSignature;
            user.MainPageContext = model.MainPageContext;
            user.Birthday = model.Birthday;
            user.Integral = model.Integral;
            user.ContributionValue = model.ContributionValue;
            user.PhotoPath = model.PhotoName;
            user.MBgImage = model.MBgImageName;
            user.SBgImage = model.SBgImageName;
            user.CanComment = model.CanComment;
            user.BackgroundImage = model.BackgroundName;
            user.IsPassedVerification = model.IsPassedVerification;
            user.IsShowFavotites = model.IsShowFavotites;

            if (await _userManager.IsInRoleAsync(user_admin, "SuperAdmin"))
            {
                //处理用户角色
                var userRoles = await _userManager.GetRolesAsync(user);
                foreach (var item in model.Roles)
                {
                    var isAdd = false;
                    foreach (var infor in userRoles)
                    {
                        if (item.Name == infor)
                        {
                            if (item.IsSelected == false)
                            {
                                await _userManager.RemoveFromRoleAsync(user, infor);
                                isAdd = true;
                                break;
                            }
                        }
                    }
                    if (isAdd == false && item.IsSelected == true)
                    {
                        await _userManager.AddToRoleAsync(user, item.Name);
                    }
                }
            }

            //更新数据
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new Result { Successful = true };
            }

            return new Result { Successful = false, Error = result.Errors.ToList()[0].Description };
        }

        /// <summary>
        /// 获取词条列表概览 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ListEntriesInforViewModel>> ListEntriesAsync()
        {
            var model = new ListEntriesInforViewModel
            {
                All = await _entryRepository.CountAsync(),
                Games = await _entryRepository.CountAsync(x => x.Type == EntryType.Game),
                Staffs = await _entryRepository.CountAsync(x => x.Type == EntryType.Staff),
                Roles = await _entryRepository.CountAsync(x => x.Type == EntryType.Role),
                Groups = await _entryRepository.CountAsync(x => x.Type == EntryType.ProductionGroup),
                Hiddens = await _entryRepository.CountAsync(x => x.IsHidden == true)
            };

            return model;
        }

        /// <summary>
        /// 获取词条列表
        /// </summary>
        /// <param name="input">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListEntryAloneModel>>> GetEntryListAsync(EntriesPagesInfor input)
        {
            var dtos = await _entryService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        /// <summary>
        /// 获取文章列表概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<GetArticleCountModel>> ListArticlesAsync()
        {
            var model = new GetArticleCountModel
            {
                All = await _articleRepository.CountAsync(),
                Toughts = await _articleRepository.CountAsync(x => x.Type == ArticleType.Tought),
                Interviews = await _articleRepository.CountAsync(x => x.Type == ArticleType.Interview),
                Strategies = await _articleRepository.CountAsync(x => x.Type == ArticleType.Strategy),
                News = await _articleRepository.CountAsync(x => x.Type == ArticleType.News),
                Hiddens = await _articleRepository.CountAsync(x => x.IsHidden == true)
            };

            return model;
        }

        /// <summary>
        /// 获取文章列表
        /// </summary>
        /// <param name="input">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListArticleAloneModel>>> GetArticleListAsync(ArticlesPagesInfor input)
        {
            var dtos = await _articleService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListExaminesInforViewModel>> ListExaminesAsync()
        {
            ListExaminesInforViewModel model = new();
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            model.All = await _examineRepository.CountAsync();
            model.Passed = await _examineRepository.CountAsync(x => x.IsPassed == true && x.PassedTime != null && x.PassedTime.Value.Date == tempDateTimeNow.Date);
            model.Unpassed = await _examineRepository.CountAsync(x => x.IsPassed == false && x.PassedTime != null && x.PassedTime.Value.Date == tempDateTimeNow.Date);
            model.Examining = await _examineRepository.CountAsync(x => x.IsPassed == null);


            return model;
        }
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListExamineAloneModel>>> GetExamineListAsync(ExaminesPagesInfor input)
        {
            var dtos = await _examineService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListCommentsInforViewModel>> ListCommentsAsync()
        {
            var model = new ListCommentsInforViewModel
            {
                All = await _commentRepository.LongCountAsync(),
                EntryComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.CommentEntries),
                ArticleComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.CommentArticle),
                SpaceComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.CommentUser),
                ParentComments = await _commentRepository.LongCountAsync(s => s.Type != CommentType.ReplyComment),
                ChildComments = await _commentRepository.LongCountAsync(s => s.Type == CommentType.ReplyComment),
                Hiddens = await _commentRepository.CountAsync(x => x.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListCommentAloneModel>>> GetCommentListAsync(CommentsPagesInfor input)
        {
            var dtos = await _commentService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListMessagesInforViewModel>> ListMessagesAsync()
        {
            var model = new ListMessagesInforViewModel
            {
                All = await _messageRepository.LongCountAsync(),
                ReadedCount = await _messageRepository.LongCountAsync(s => s.IsReaded == true),
                NotReadedCount = await _messageRepository.LongCountAsync(s => s.IsReaded == false)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListMessageAloneModel>>> GetMessageListAsync(MessagesPagesInfor input)
        {
            var dtos = await _messageService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListFilesInforViewModel>> ListFilesAsync()
        {
            var model = new ListFilesInforViewModel
            {
                All = await _userFileRepository.CountAsync()
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFileAloneModel>>> GetFileListAsync(FilesPagesInfor input)
        {
            var dtos = await _fileService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListErrorCountsInforViewModel>> ListErrorCountsAsync()
        {
            var model = new ListErrorCountsInforViewModel
            {
                All = await _errorCountRepository.LongCountAsync()
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListErrorCountAloneModel>>> GetErrorCountListAsync(ErrorCountsPagesInfor input)
        {
            var dtos = await _errorCountService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListFavoriteFoldersInforViewModel>> ListFavoriteFoldersAsync()
        {
            var model = new ListFavoriteFoldersInforViewModel
            {
                All = await _favoriteFolderRepository.LongCountAsync(),
                Defaults = await _favoriteFolderRepository.LongCountAsync(s => s.IsDefault == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteFolderAloneModel>>> GetFavoriteFolderListAsync(FavoriteFoldersPagesInfor input)
        {
            var dtos = await _favoriteFolderService.GetPaginatedResult(input.Options, input.SearchModel, null);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListRanksInforViewModel>> ListRanksAsync()
        {
            var model = new ListRanksInforViewModel
            {
                All = await _rankRepository.LongCountAsync(),
                Hiddens = await _rankRepository.LongCountAsync(s => s.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRankAloneModel>>> GetRankListAsync(RanksPagesInfor input)
        {
            var dtos = await _rankService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListPeripheriesInforViewModel>> ListPeripheriesAsync()
        {
            var model = new ListPeripheriesInforViewModel
            {
                All = await _peripheryRepository.LongCountAsync(),
                Hiddens = await _peripheryRepository.LongCountAsync(s => s.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListPeripheryAloneModel>>> GetPeripheryListAsync(PeripheriesPagesInfor input)
        {
            var dtos = await _peripheryService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListVotesInforViewModel>> ListVotesAsync()
        {
            var model = new ListVotesInforViewModel
            {
                All = await _rankRepository.LongCountAsync(),
                Hiddens = await _rankRepository.LongCountAsync(s => s.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListVoteAloneModel>>> GetVoteListAsync(VotesPagesInfor input)
        {
            var dtos = await _voteService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        /// <summary>
        /// 获取编辑记录详细信息视图
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [HttpGet]
        public async Task<ActionResult<Models.ExaminedViewModel>> ExaminedAsync(int id = -1)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取审核单
            Examine examine = null;
            var IsContinued = false;

            if (id == -1)
            {
                try
                {
                    examine = await _examineRepository.GetAll()
                        .Include(s => s.ApplicationUser)
                        .FirstOrDefaultAsync(x => x.IsPassed == null);
                    if (examine == null)
                    {
                        return NotFound();
                    }
                    IsContinued = true;
                }
                catch
                {
                    return NotFound();
                }
            }
            else
            {
                examine = await _examineRepository.GetAll()
                        .Include(s => s.ApplicationUser)
                        .FirstOrDefaultAsync(x => x.Id == id);
            }

            if (examine == null)
            {
                return NotFound();
            }
            //对应赋值
            var model = new Models.ExaminedViewModel
            {
                Id = examine.Id,
                ApplicationUserId = examine.ApplicationUserId,
                ApplyTime = examine.ApplyTime,
                Operation = examine.Operation,
                ApplicationUserName = examine.ApplicationUser.UserName,
                Comments = examine.Comments,
                IsPassed = false,
                IsContinued = IsContinued,
                IsExamined = false,
                PassedAdminName = examine.PassedAdminName,
                Note = examine.Note
            };
            //判断是否有前置审核
            if (examine.PrepositionExamineId != null)
            {
                var temp = await _examineRepository.FirstOrDefaultAsync(s => s.Id == examine.PrepositionExamineId && s.IsPassed == null);
                if (temp == null)
                {
                    model.PrepositionExamineId = -1;
                }
                else
                {
                    model.PrepositionExamineId = (int)examine.PrepositionExamineId;
                }
            }
            //判断是否是等待审核状态
            if (examine.IsPassed != null)
            {
                model.PassedTime = (DateTime)examine.PassedTime;
                model.IsExamined = true;
                model.IsPassed = (bool)examine.IsPassed;
            }
            try
            {
                if (await _examineService.GetExamineView(model, examine) == false)
                {
                    return NotFound();
                }

            }
            catch (Exception)
            {
                return NotFound("生成审核视图出错");
            }
            //获取敏感词列表
            if (string.IsNullOrWhiteSpace(examine.Context) == false && user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                model.SensitiveWords = await _appHelper.GetSensitiveWordsInText(examine.Context);
            }
            return model;
        }

        /// <summary>
        /// 对编辑进行审核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ExaminedAsync(Models.ExaminedViewModel model)
        {
            var examine = await _examineRepository.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (examine == null)
            {
                return NotFound();
            }
            if (examine.IsPassed != null)
            {
                return new Result { Successful = false, Error = "该记录已经被被审核，不能修改审核状态" };
            }
            var user = await _userManager.Users.SingleAsync(x => x.Id == examine.ApplicationUserId);
            if (user == null)
            {
                return NotFound();
            }
            //判断是否有前置审核
            if (examine.PrepositionExamineId != null)
            {
                var temp = await _examineRepository.FirstOrDefaultAsync(s => s.Id == examine.PrepositionExamineId && s.IsPassed == null);
                if (temp != null)
                {
                    return new Result { Successful = false, Error = $"该审核有一个前置审核,请先对前置审核进行审核，ID{examine.PrepositionExamineId}" };
                }
            }
            Entry entry = null;
            Article article = null;
            Comment comment = null;
            Tag tag = null;
            Disambig disambig = null;
            Periphery periphery = null;
            //获取当前管理员ID
            var userAdmin = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //判断是否通过
            if (model.IsPassed == true)
            {

                //根据操作类别进行操作
                switch (examine.Operation)
                {
                    case Operation.UserMainPage:
                        await _examineService.ExamineEditUserMainPageAsync(user, examine.Context);
                        break;
                    case Operation.EditUserMain:

                        //序列化数据
                        UserMain userMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            userMain = (UserMain)serializer.Deserialize(str, typeof(UserMain));
                        }

                        await _examineService.ExamineEditUserMainAsync(user, userMain);
                        break;
                    case Operation.EstablishMain:
                        entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                        if (entry == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        ExamineMain examineMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            examineMain = (ExamineMain)serializer.Deserialize(str, typeof(ExamineMain));
                        }

                        await _examineService.ExamineEstablishMainAsync(entry, examineMain);
                        break;
                    case Operation.EstablishAddInfor:
                        entry = await _entryRepository.GetAll()
                            .Include(s => s.Information)
                              .ThenInclude(s => s.Additional)
                            .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                        if (entry == null)
                        {
                            return NotFound();
                        }

                        //读取当前用户等待审核的信息
                        //序列化数据
                        EntryAddInfor entryAddInfor = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            entryAddInfor = (EntryAddInfor)serializer.Deserialize(str, typeof(EntryAddInfor));
                        }
                        await _examineService.ExamineEstablishAddInforAsync(entry, entryAddInfor);
                        break;
                    case Operation.EstablishImages:
                        entry = await _entryRepository.GetAll()
                           .Include(s => s.Pictures)
                           .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                        if (entry == null)
                        {
                            return NotFound();
                        }

                        //序列化数据
                        EntryImages entryImages = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            entryImages = (EntryImages)serializer.Deserialize(str, typeof(EntryImages));
                        }

                        await _examineService.ExamineEstablishImagesAsync(entry, entryImages);
                        break;
                    case Operation.EstablishRelevances:

                        entry = await _entryRepository.GetAll()
                    .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                            .FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                        if (entry == null)
                        {
                            return NotFound();
                        }

                        EntryRelevances entryRelevances = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            entryRelevances = (EntryRelevances)serializer.Deserialize(str, typeof(EntryRelevances));
                        }
                        await _examineService.ExamineEstablishRelevancesAsync(entry, entryRelevances);
                        break;
                    case Operation.EstablishTags:

                        entry = await _entryRepository.GetAll().Include(s => s.Tags).FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                        if (entry == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        EntryTags entryTags = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            entryTags = (EntryTags)serializer.Deserialize(str, typeof(EntryTags));
                        }
                        await _examineService.ExamineEstablishTagsAsync(entry, entryTags);
                        break;
                    case Operation.EstablishMainPage:

                        entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                        if (entry == null)
                        {
                            return NotFound();
                        }
                        await _examineService.ExamineEstablishMainPageAsync(entry, examine.Context);
                        break;
                    case Operation.EditArticleMain:

                        article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                        if (article == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        ExamineMain articleMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            articleMain = (ExamineMain)serializer.Deserialize(str, typeof(ExamineMain));
                        }

                        await _examineService.ExamineEditArticleMainAsync(article, articleMain);
                        break;
                    case Operation.EditArticleRelevanes:

                        article = await _articleRepository.GetAll()
                            .Include(s => s.ArticleRelationFromArticleNavigation).ThenInclude(s => s.ToArticleNavigation)
                            .FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                        if (article == null)
                        {
                            return NotFound();
                        }

                        ArticleRelevances articleRelevances = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            articleRelevances = (ArticleRelevances)serializer.Deserialize(str, typeof(ArticleRelevances));
                        }
                        await _examineService.ExamineEditArticleRelevancesAsync(article, articleRelevances);
                        break;
                    case Operation.EditArticleMainPage:

                        article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                        if (article == null)
                        {
                            return NotFound();
                        }
                        await _examineService.ExamineEditArticleMainPageAsync(article, examine.Context);
                        break;
                    case Operation.EditTagMain:
                        tag = await _tagRepository.GetAll().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
                        if (tag == null)
                        {
                            return NotFound();
                        }
                        TagMain tagMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            tagMain = (TagMain)serializer.Deserialize(str, typeof(TagMain));
                        }
                        await _examineService.ExamineEditTagMainAsync(tag, tagMain);
                        break;
                    case Operation.EditTagChildTags:
                        tag = await _tagRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == examine.TagId);
                        if (tag == null)
                        {
                            return NotFound();
                        }
                        TagChildTags tagChildTags = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            tagChildTags = (TagChildTags)serializer.Deserialize(str, typeof(TagChildTags));
                        }
                        await _examineService.ExamineEditTagChildTagsAsync(tag, tagChildTags);
                        break;
                    case Operation.EditTagChildEntries:
                        tag = await _tagRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == examine.TagId);
                        if (tag == null)
                        {
                            return NotFound();
                        }
                        TagChildEntries tagChildEntries = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            tagChildEntries = (TagChildEntries)serializer.Deserialize(str, typeof(TagChildEntries));
                        }
                        await _examineService.ExamineEditTagChildEntriesAsync(tag, tagChildEntries);
                        break;
                    case Operation.PubulishComment:
                        comment = await _commentRepository.FirstOrDefaultAsync(s => s.Id == examine.CommentId);
                        if (comment == null)
                        {
                            return NotFound();
                        }
                        CommentText commentText = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            commentText = (CommentText)serializer.Deserialize(str, typeof(CommentText));
                        }
                        await _appHelper.ExaminePublishCommentTextAsync(comment, commentText);
                        break;
                    case Operation.DisambigMain:
                        disambig = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
                        if (disambig == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        DisambigMain disambigMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            disambigMain = (DisambigMain)serializer.Deserialize(str, typeof(DisambigMain));
                        }

                        await _examineService.ExamineEditDisambigMainAsync(disambig, disambigMain);
                        break;
                    case Operation.DisambigRelevances:
                        disambig = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
                        if (disambig == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        DisambigRelevances disambigRelevances = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            disambigRelevances = (DisambigRelevances)serializer.Deserialize(str, typeof(DisambigRelevances));
                        }

                        await _examineService.ExamineEditDisambigRelevancesAsync(disambig, disambigRelevances);
                        break;
                    case Operation.EditPeripheryMain:
                        periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                        if (periphery == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        PeripheryMain peripheryMain = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            peripheryMain = (PeripheryMain)serializer.Deserialize(str, typeof(PeripheryMain));
                        }

                        await _examineService.ExamineEditPeripheryMainAsync(periphery, peripheryMain);
                        break;
                    case Operation.EditPeripheryImages:
                        periphery = await _peripheryRepository.GetAll().Include(s => s.Pictures).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                        if (periphery == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        PeripheryImages peripheryImages = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            peripheryImages = (PeripheryImages)serializer.Deserialize(str, typeof(PeripheryImages));
                        }

                        await _examineService.ExamineEditPeripheryImagesAsync(periphery, peripheryImages);
                        break;
                    case Operation.EditPeripheryRelatedEntries:
                        periphery = await _peripheryRepository.GetAll().AsNoTracking().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                        if (periphery == null)
                        {
                            return NotFound();
                        }
                        //序列化数据
                        PeripheryRelatedEntries peripheryRelevances = null;
                        using (TextReader str = new StringReader(examine.Context))
                        {
                            var serializer = new JsonSerializer();
                            peripheryRelevances = (PeripheryRelatedEntries)serializer.Deserialize(str, typeof(PeripheryRelatedEntries));
                        }

                        await _examineService.ExamineEditPeripheryRelatedEntriesAsync(periphery, peripheryRelevances);
                        break;
                }

                //修改审核状态
                examine.Comments = model.Comments;
                examine.PassedAdminName = userAdmin.UserName;
                examine.PassedTime = DateTime.Now.ToCstTime();
                examine.IsPassed = true;
                examine.ContributionValue = model.ContributionValue;
                examine = await _examineRepository.UpdateAsync(examine);

                //更新用户积分
                await _appHelper.UpdateUserIntegral(user);
                await _rankService.UpdateUserRanks(user);

            }
            else
            {
                entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                tag = await _tagRepository.FirstOrDefaultAsync(s => s.Id == examine.TagId);
                comment = await _commentRepository.FirstOrDefaultAsync(s => s.Id == examine.CommentId);
                disambig = await _disambigRepository.FirstOrDefaultAsync(s => s.Id == examine.DisambigId);
                periphery = await _peripheryRepository.FirstOrDefaultAsync(s => s.Id == examine.PeripheryId);
                //修改审核状态
                examine.Comments = model.Comments;
                examine.PassedAdminName = userAdmin.UserName;
                examine.PassedTime = DateTime.Now.ToCstTime();
                examine.IsPassed = false;
                examine = await _examineRepository.UpdateAsync(examine);
                //修改以其为前置审核的审核状态
                if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                {
                    var temp = _examineRepository.GetAll().Where(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id);
                    foreach (var item in temp)
                    {
                        item.IsPassed = false;
                        item.Comments = "其前置审核被驳回";
                        item.PassedTime = DateTime.Now.ToCstTime();
                        item.PassedAdminName = userAdmin.UserName;
                        await _examineRepository.UpdateAsync(item);
                    }
                }
            }
            //给用户发送通知
            if (article != null)
            {
                if (examine.PrepositionExamineId == null || examine.PrepositionExamineId <= 0)
                {
                    //查找是否有以此为前置审核的审核 如果有 则代表第一次创建
                    if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "文章发布成功提醒" : "文章发布驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你的文章『" + article.Name + "』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "审核通过，已经成功发布，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "articles/index/" + examine.Article.Id,
                            LinkTitle = article.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                    else
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你对文章『" + article.Name + "』的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "articles/index/" + article.Id,
                            LinkTitle = article.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                }

            }
            else if (entry != null)
            {
                if (examine.PrepositionExamineId == null || examine.PrepositionExamineId <= 0)
                {
                    //查找是否有以此为前置审核的审核 如果有 则代表第一此创建
                    if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "词条创建成功提醒" : "词条创建驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你创建的词条『" + entry.Name + "』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "审核通过，已经可以被浏览，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "entries/index/" + entry.Id,
                            LinkTitle = entry.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                    else
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你对词条『" + entry.Name + "』的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "entries/index/" + entry.Id,
                            LinkTitle = entry.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                }
            }
            else if (periphery != null)
            {
                if (examine.PrepositionExamineId == null || examine.PrepositionExamineId <= 0)
                {
                    //查找是否有以此为前置审核的审核 如果有 则代表第一此创建
                    if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "周边创建成功提醒" : "周边创建驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你创建的周边『" + periphery.Name + "』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "审核通过，已经可以被浏览，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "peripheries/index/" + periphery.Id,
                            LinkTitle = periphery.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                    else
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你对周边『" + periphery.Name + "』的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "peripheries/index/" + periphery.Id,
                            LinkTitle = periphery.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                }
            }
            else if (disambig != null)
            {
                if (examine.PrepositionExamineId == null || examine.PrepositionExamineId <= 0)
                {
                    //查找是否有以此为前置审核的审核 如果有 则代表第一此创建
                    if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "消歧义页面创建成功提醒" : "消歧义页面创建驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你创建的消歧义页面『" + disambig.Name + "』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "审核通过，已经可以被浏览，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "disambigs/index/" + disambig.Id,
                            LinkTitle = disambig.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                    else
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你对消歧义页面『" + disambig.Name + "』的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "disambigs/index/" + disambig.Id,
                            LinkTitle = examine.Entry.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                }
            }
            else if (tag != null)
            {
                if (examine.PrepositionExamineId == null || examine.PrepositionExamineId <= 0)
                {
                    //查找是否有以此为前置审核的审核 如果有 则代表第一此创建
                    if (await _examineRepository.CountAsync(s => s.IsPassed == null && s.PrepositionExamineId == examine.Id) != 0)
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "标签创建成功提醒" : "标签创建驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你创建的标签『" + tag.Name + "』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "审核通过，已经可以被浏览，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "tags/index/" + tag.Id,
                            LinkTitle = tag.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                    else
                    {
                        await _messageRepository.InsertAsync(new Message
                        {
                            Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                            PostTime = DateTime.Now.ToCstTime(),
                            Image = "default/logo.png",
                            Rank = "系统",
                            Text = "你对标签『" + tag.Name + "』的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                            Link = "tags/index/" + tag.Id,
                            LinkTitle = tag.Name,
                            Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                            ApplicationUser = user,
                            ApplicationUserId = user.Id
                        });
                    }
                }
            }
            else if (comment != null)
            {
                await _messageRepository.InsertAsync(new Message
                {
                    Title = (examine.IsPassed ?? false) ? "评论审核通过提醒" : "评论驳回提醒",
                    PostTime = DateTime.Now.ToCstTime(),
                    Image = "default/logo.png",
                    Rank = "系统",
                    Text = "你的评论『\n" + _appHelper.GetStringAbbreviation(comment.Text, 20) + "\n』被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                    Link = "",
                    LinkTitle = "",
                    Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                    ApplicationUser = user,
                    ApplicationUserId = user.Id
                });
            }
            else
            {
                await _messageRepository.InsertAsync(new Message
                {
                    Title = (examine.IsPassed ?? false) ? "编辑通过提醒" : "编辑驳回提醒",
                    PostTime = DateTime.Now.ToCstTime(),
                    Image = "default/logo.png",
                    Rank = "系统",
                    Text = "你的『" + examine.Operation.GetDisplayName() + "』操作被管理员『" + userAdmin.UserName + "』" + ((examine.IsPassed ?? false) ? "通过，感谢你为CnGal资料站做出的贡献" : "驳回") + (string.IsNullOrWhiteSpace(examine.Comments) ? "" : "，批注『" + examine.Comments + "』"),
                    Link = "home/examined/" + examine.Id,
                    LinkTitle = "第" + examine.Id + "条审核记录",
                    Type = (examine.IsPassed ?? false) ? MessageType.ExaminePassed : MessageType.ExamineUnPassed,
                    ApplicationUser = user,
                    ApplicationUserId = user.Id
                });

            }

            return new Result { Successful = true };
        }

        /// <summary>
        /// 管理主页 包括友情链接 轮播图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ManageHomeViewModel>> ManageHomeAsync()
        {
            var model = new ManageHomeViewModel
            {
                Links = await _friendLinkRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync()
            };
            foreach (var item in model.Links)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "");
            }
            model.Carousels = await _carouselRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in model.Carousels)
            {
                item.Image = _appHelper.GetImagePath(item.Image, "");
            }


            model.AppImage = _appHelper.GetImagePath("", "app.png");
            model.UserImage = _appHelper.GetImagePath("", "user.png");
            model.UserBackgroundImage = _appHelper.GetImagePath("", "userbackground.jpg");
            model.CertificateImage = _appHelper.GetImagePath("", "certificate.png");
            model.BackgroundImage = _appHelper.GetImagePath("", "background.png");

            return model;
        }

        /// <summary>
        /// 编辑轮播图 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<EditCarouselsViewModel>> EditCarouselsAsync()
        {
            //根据类别生成首个视图模型
            var model = new EditCarouselsViewModel
            {
                Carousels = new List<CarouselModel>()
            };
            var carousels = await _carouselRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in carousels)
            {
                model.Carousels.Add(new CarouselModel
                {
                    Link = item.Link,
                    Priority = item.Priority,
                    ImagePath = _appHelper.GetImagePath(item.Image, "")
                });

            }

            return model;
        }

        /// <summary>
        /// 编辑轮播图
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> EditCarouselsAsync(EditCarouselsViewModel model)
        {
            //先把删除当前所有图片
            var carousels = await _carouselRepository.GetAll().ToListAsync();
            foreach (var item in carousels)
            {
                //_appHelper.DeleteImage(item.Image);
                await _carouselRepository.DeleteAsync(item);
            }
            //循环添加视图中的图片
            if (model.Carousels != null)
            {
                foreach (var item in model.Carousels)
                {
                    await _carouselRepository.InsertAsync(new Carousel
                    {
                        Image = item.ImagePath == "background.png" ? "" : item.ImagePath,
                        Link = item.Link,
                        Priority = item.Priority
                    });
                }
            }
            return new Result { Successful = true };
        }

        /// <summary>
        /// 编辑友情链接
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<EditFriendLinksViewModel>> EditFriendLinksAsync()
        {
            //根据类别生成首个视图模型
            var model = new EditFriendLinksViewModel
            {
                FriendLinks = new List<FriendLinkModel>()
            };
            var friendLinks = await _friendLinkRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in friendLinks)
            {
                model.FriendLinks.Add(new FriendLinkModel
                {
                    Link = item.Link,
                    Name = item.Name,
                    Priority = item.Priority,
                    ImagePath = _appHelper.GetImagePath(item.Image, "app.png")
                });
            }
            return model;
        }

        /// <summary>
        /// 编辑友情链接
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> EditFriendLinksAsync(EditFriendLinksViewModel model)
        {

            //先把删除当前所有图片
            var friendLinks = await _friendLinkRepository.GetAll().ToListAsync();
            foreach (var item in friendLinks)
            {
                //_appHelper.DeleteImage(item.Image);
                await _friendLinkRepository.DeleteAsync(item);
            }
            //循环添加视图中的图片
            if (model.FriendLinks != null)
            {
                foreach (var item in model.FriendLinks)
                {
                    await _friendLinkRepository.InsertAsync(new FriendLink
                    {
                        Image = item.ImagePath == "background.png" ? "" : item.ImagePath,
                        Name = item.Name,
                        Link = item.Link,
                        Priority = item.Priority
                    });
                }
            }
            return new Result { Successful = true };
        }

        /// <summary>
        /// 置顶最新发行的游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> MakeNewestGamesTop()
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            //获取所有游戏
            var entriesList = _entryRepository.GetAll().Include(s => s.Information).Where(s => s.Type == EntryType.Game);

            //获取游戏发行时间
            var gameTimes = new Dictionary<int, DateTime>();
            foreach (var item in entriesList)
            {
                foreach (var temp in item.Information)
                {

                    if (temp.Modifier == "基本信息" && temp.DisplayName == "发行时间")
                    {
                        try
                        {
                            gameTimes.Add(item.Id, DateTime.ParseExact(temp.DisplayValue, "yyyy年M月d日", null));
                        }
                        catch
                        {
                            try
                            {
                                gameTimes.Add(item.Id, DateTime.ParseExact(temp.DisplayValue, "yyyy/M/d", null));
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                }
            }

            //对其进行排序
            var gameTimes_Sorted = gameTimes.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, o => o.Value);
            //取前几个修改权重 从10开始
            var num = 6;
            foreach (var item in gameTimes_Sorted)
            {
                //获取词条
                var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Id == item.Key);
                if (entry != null)
                {
                    entry.Priority = 10 + num;
                    await _entryRepository.UpdateAsync(entry);
                    num--;
                }
                else
                {
                    return NotFound();
                }
                if (num < 1)
                {
                    break;
                }
            }
            return new Result { Successful = true };
        }


        /// <summary>
        /// 临时脚本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> TempFunction()
        {
            try
            {
                var freegames =await _steamInforRepository.GetAll().Where(s => s.PriceNow == 0).Include(s => s.Entry).Select(s => s.Entry).ToListAsync();
                var tag = await _tagRepository.GetAll().Include(s=>s.Entries).FirstOrDefaultAsync(s => s.Name == "免费");
                tag.Entries = freegames;
                await _tagRepository.UpdateAsync(tag);
                //string temp= await _fileService.SaveImageAsync("https://wx4.sinaimg.cn/mw2000/008qAv3ngy1gyem1zkfwqj31cr0s9hbg.jpg", _configuration["NewsAdminId"]);
                //await _elasticsearchService.DeleteDataOfElasticsearch();
                //await _elasticsearchService.UpdateDataToElasticsearch(DateTime.MinValue);

                //await _weeklyNewsRepository.DeleteAsync(s => true);
                /*var news = await _gameNewsRepository.GetAll().Include(s => s.RSS).Where(s => s.State == GameNewsState.Ignore).ToListAsync();
                foreach (var item in news)
                {
                    item.Title = "已删除";
                    item.PublishTime = DateTime.MinValue;
                    item.RSS.PublishTime = DateTime.MinValue;
                    await _gameNewsRepository.UpdateAsync(item);
                }*/

                //await _historyDataService.GenerateZhiHuArticleImportJson();



                return new Result { Successful = true };
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source };
            }

        }


        /// <summary>
        /// 获取网站数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<OverviewDataViewModel>> GetOverviewData()
        {
            try
            {
                var tempDateTimeNow = DateTime.Now.ToCstTime();
                //获取数据
                var model = new OverviewDataViewModel
                {
                    TotalRegisterCount = await _userRepository.CountAsync(),
                    YesterdayRegisterCount = await _userRepository.CountAsync(s => s.RegistTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayRegisterCount = await _userRepository.CountAsync(s => s.RegistTime.Date == tempDateTimeNow.Date),
                    TodayOnlineCount = await _userRepository.CountAsync(s => s.LastOnlineTime.Date == tempDateTimeNow.Date),

                    TotalEntryCount = await _entryRepository.CountAsync(),
                    YesterdayEditEntryCount = await _examineRepository.CountAsync(s => s.IsPassed == true && s.PassedTime != null && s.PassedTime.Value.Date == tempDateTimeNow.AddDays(-1).Date && (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishMainPage)),
                    TodayEditEntryCount = await _entryRepository.CountAsync(s => s.LastEditTime.Date == tempDateTimeNow.Date),

                    TotalArticleCount = await _articleRepository.LongCountAsync(),
                    YesterdayCreateArticleCount = await _articleRepository.LongCountAsync(s => s.CreateTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    YesterdayEditArticleCount = await _examineRepository.LongCountAsync(s => s.IsPassed == true && s.PassedTime != null && s.PassedTime.Value.Date == tempDateTimeNow.AddDays(-1).Date && (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleRelevanes || s.Operation == Operation.EditArticleMainPage)),
                    TodayCreateArticleCount = await _articleRepository.LongCountAsync(s => s.CreateTime.Date == tempDateTimeNow.Date),
                    TodayEditArticleCount = await _articleRepository.LongCountAsync(s => s.LastEditTime.Date == tempDateTimeNow.Date),

                    TotalTagCount = await _tagRepository.CountAsync(),
                    YesterdayEditTagCount = await _examineRepository.CountAsync(s => s.IsPassed == true && s.PassedTime != null && s.PassedTime.Value.Date == tempDateTimeNow.AddDays(-1).Date && (s.Operation == Operation.EditTag)),
                    TodayEditTagCount = await _examineRepository.CountAsync(s => s.IsPassed == true && s.PassedTime != null && s.PassedTime.Value.Date == tempDateTimeNow.Date && (s.Operation == Operation.EditTag)),

                    TotalExamineCount = await _examineRepository.LongCountAsync(),
                    YesterdayTotalExamineCount = await _examineRepository.LongCountAsync(s => s.ApplyTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayTotalExamineCount = await _examineRepository.LongCountAsync(s => s.ApplyTime.Date == tempDateTimeNow.Date),
                    TotalExaminingCount = await _examineRepository.LongCountAsync(s => s.IsPassed == null),

                    TotalCommentCount = await _commentRepository.LongCountAsync(),
                    YesterdayCommentCount = await _commentRepository.LongCountAsync(s => s.CommentTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayCommentCount = await _commentRepository.LongCountAsync(s => s.CommentTime.Date == tempDateTimeNow.Date),

                    TotalMessageCount = await _messageRepository.LongCountAsync(),
                    YesterdayMessageCount = await _messageRepository.LongCountAsync(s => s.PostTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    TodayMessageCount = await _messageRepository.LongCountAsync(s => s.PostTime.Date == tempDateTimeNow.Date),

                    TotalFileCount = await _userFileRepository.CountAsync(),
                    TotalFileSpace = await _userFileRepository.GetAll().SumAsync(s => s.FileSize) ?? 0,
                    YesterdayFileCount = await _userFileRepository.CountAsync(s => s.UploadTime.Date == tempDateTimeNow.AddDays(-1).Date),
                    YesterdayFileSpace = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date == tempDateTimeNow.AddDays(-1).Date).SumAsync(s => s.FileSize) ?? 0,
                    TodayFileCount = await _userFileRepository.CountAsync(s => s.UploadTime.Date == tempDateTimeNow.Date),
                    TodayFileSpace = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date == tempDateTimeNow.Date).SumAsync(s => s.FileSize) ?? 0,
                };

                return model;
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }

        }
        /// <summary>
        /// 图表天数
        /// </summary>
        public const int MaxCountLineDay = 30;

        /// <summary>
        /// 获取用户图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetUserCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            //获取数据
            var registerCounts = await _userRepository.GetAll().Where(s => s.RegistTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.RegistTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var onlineCounts = await _userOnlineInforRepository.GetAll().Where(s => s.Date.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
             // 先进行了时间字段变更为String字段，切只保留到天
             // 采用拼接的方式
             .Select(n => new { Time = n.Date.Date })
             // 分类
             .GroupBy(n => n.Time)
             // 返回汇总样式
             .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
             .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
             .ToListAsync();


            var SignIns = await _signInDayRepository.GetAll().Where(s => s.Time.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
             // 先进行了时间字段变更为String字段，切只保留到天
             // 采用拼接的方式
             .Select(n => new { Time = n.Time.Date })
             // 分类
             .GroupBy(n => n.Time)
             // 返回汇总样式
             .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
             .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
             .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["注册"] = registerCounts, ["在线"] = onlineCounts, ["签到"] = SignIns }, "日期", "数目", "用户");
            return temp;
        }
        /// <summary>
        /// 获取词条图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetEntryCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            //获取数据
            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EstablishMain || s.Operation == Operation.EstablishAddInfor || s.Operation == Operation.EstablishImages || s.Operation == Operation.EstablishRelevances || s.Operation == Operation.EstablishTags || s.Operation == Operation.EstablishMainPage)
                                                                           && s.IsPassed == true && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var commentCounts = await _commentRepository.GetAll().Where(s => s.Type == CommentType.CommentEntries && s.CommentTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CommentTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            //获取数据
            var favoriteCounts = await _favoriteObjectRepository.GetAll().Where(s => s.Type == FavoriteObjectType.Entry && s.CreateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["编辑"] = editCounts, ["评论"] = commentCounts, ["收藏"] = favoriteCounts }, "日期", "数目", "词条");
            return temp;
        }

        /// <summary>
        /// 获取文章图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetArticleCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var createCounts = await _articleRepository.GetAll().Where(s => s.CreateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditArticleMain || s.Operation == Operation.EditArticleRelevanes || s.Operation == Operation.EditArticleMainPage)
                                                                           && s.IsPassed == true && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();
            var thumsupCounts = await _thumbsUpRepository.GetAll().Where(s => s.ThumbsUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ThumbsUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var commentCounts = await _commentRepository.GetAll().Where(s => s.Type == CommentType.CommentArticle && s.CommentTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
              // 先进行了时间字段变更为String字段，切只保留到天
              // 采用拼接的方式
              .Select(n => new { Time = n.CommentTime.Date })
              // 分类
              .GroupBy(n => n.Time)
              // 返回汇总样式
              .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
              .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
              .ToListAsync();

            var favoriteCounts = await _favoriteObjectRepository.GetAll().Where(s => s.Type == FavoriteObjectType.Article && s.CreateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CreateTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["发表"] = createCounts, ["编辑"] = editCounts, ["点赞"] = thumsupCounts, ["评论"] = commentCounts, ["收藏"] = favoriteCounts }, "日期", "数目", "文章");
            return temp;
        }

        /// <summary>
        /// 获取标签图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetTagCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var editCounts = await _examineRepository.GetAll().Where(s => (s.Operation == Operation.EditTag || s.Operation == Operation.EditTagMain || s.Operation == Operation.EditTagChildTags || s.Operation == Operation.EditTagChildEntries)
                                                                           && s.IsPassed == true && s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();


            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["编辑"] = editCounts }, "日期", "数目", "标签");
            return temp;
        }

        /// <summary>
        /// 获取审核图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetExamineCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var applyCounts = await _examineRepository.GetAll().Where(s => s.ApplyTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.ApplyTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var passCounts = await _examineRepository.GetAll().Where(s => s.PassedTime != null && s.PassedTime.Value.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.PassedTime.Value.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["申请"] = applyCounts, ["处理"] = passCounts }, "日期", "数目", "审核");
            return temp;
        }

        /// <summary>
        /// 获取评论图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetCommentCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var publishCounts = await _commentRepository.GetAll().Where(s => s.CommentTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.CommentTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["发表"] = publishCounts }, "日期", "数目", "评论");
            return temp;
        }


        /// <summary>
        /// 获取评论图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetMessageCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var postCounts = await _messageRepository.GetAll().Where(s => s.PostTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.PostTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["发送"] = postCounts }, "日期", "数目", "消息");
            return temp;
        }

        /// <summary>
        /// 获取文件图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetFileCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var Counts = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.UploadTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var Spaces = await _userFileRepository.GetAll().Where(s => s.UploadTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.UploadTime.Date, Space = ((double)n.FileSize) / (1024 * 1024) })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Sum(s => s.Space) })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["数目"] = Counts, ["大小 MB"] = Spaces }, "日期", "", "文件");
            return temp;
        }

        /// <summary>
        /// 获取备份图表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetBackUpArchiveCountLineAsync()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            //获取数据
            var success = await _backUpArchiveDetailRepository.GetAll().Where(s => s.IsFail == false && s.BackUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var times = await _backUpArchiveDetailRepository.GetAll().Where(s => s.BackUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date, n.TimeUsed })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Average(s => s.TimeUsed) })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var errors = await _backUpArchiveDetailRepository.GetAll().Where(s => s.IsFail == true && s.BackUpTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.BackUpTime.Date })
               // 分类
               .GroupBy(n => n.Time)
               // 返回汇总样式
               .Select(n => new CountLineModel { Time = n.Key, Count = n.Count() })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["成功"] = success, ["错误"] = errors, ["用时 秒"] = times }, "日期", "", "备份");
            return temp;
        }
    }
}
