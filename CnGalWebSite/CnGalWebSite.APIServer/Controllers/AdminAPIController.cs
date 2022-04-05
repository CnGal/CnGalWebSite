using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.HistoryData;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Votes;
using CnGalWebSite.APIServer.Application.WeiXin;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
        private readonly ILotteryService _lotteryService;
        private readonly IWeiXinService _weiXinService;
        private readonly INewsService _newsService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly IHistoryDataService _historyDataService;
        private readonly ISearchHelper _searchHelper;
        private readonly IConfiguration _configuration;
        private readonly IRepository<LotteryUser, long> _lotteryUserRepository;
        private readonly IRepository<LotteryAward, long> _lotteryAwardRepository;
        private readonly IRepository<LotteryPrize, long> _lotteryPrizeRepository;

        public AdminAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRankService rankService, IHistoryDataService historyDataService,
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IWeiXinService weiXinService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository, IPerfectionService perfectionService, 
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, 
        IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService, IRepository<Rank, long> rankRepository, INewsService newsService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository,
        IVoteService voteService, IRepository<Vote, long> voteRepository, IRepository<SteamInfor, long> steamInforRepository, ILotteryService lotteryService,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IConfiguration configuration, IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository,
        IRepository<LotteryAward, long> lotteryAwardRepository, ISearchHelper searchHelper,
        IRepository<LotteryPrize, long> lotteryPrizeRepository)
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
            _newsService = newsService;
            _gameNewsRepository = gameNewsRepository;
            _weeklyNewsRepository = weeklyNewsRepository;
            _historyDataService = historyDataService;
            _voteService = voteService;
            _voteRepository = voteRepository;
            _configuration = configuration;
            _steamInforRepository = steamInforRepository;
            _lotteryRepository = lotteryRepository;
            _lotteryService = lotteryService;
            _lotteryUserRepository = lotteryUserRepository;
            _lotteryAwardRepository = lotteryAwardRepository;
            _lotteryPrizeRepository = lotteryPrizeRepository;
            _searchHelper = searchHelper;
            _weiXinService= weiXinService;
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
                All = await _voteRepository.LongCountAsync(),
                Hiddens = await _voteRepository.LongCountAsync(s => s.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListVoteAloneModel>>> GetVoteListAsync(VotesPagesInfor input)
        {
            var dtos = await _voteService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpGet]
        public async Task<ActionResult<ListLotteriesInforViewModel>> ListLotteriesAsync()
        {
            var model = new ListLotteriesInforViewModel
            {
                All = await _lotteryRepository.LongCountAsync(),
                Hiddens = await _lotteryRepository.LongCountAsync(s => s.IsHidden == true)
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListLotteryAloneModel>>> GetLotteryListAsync(LotteriesPagesInfor input)
        {
            var dtos = await _lotteryService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
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
        /// 刷新ES缓存
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> RefreshSearchData()
        {
            try
            {
                await _searchHelper.DeleteDataOfSearchService();
                await _searchHelper.UpdateDataToSearchService(DateTime.MinValue);

                return new Result { Successful = true };
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source };
            }

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
                _weiXinService.CreateMenu();
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
