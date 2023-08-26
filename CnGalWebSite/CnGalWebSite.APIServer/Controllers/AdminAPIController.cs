using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Charts;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.APIServer.Application.Votes;
using CnGalWebSite.APIServer.Application.WeiXin;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.APIServer.Model;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Tables;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/admin/[action]")]
    public class AdminAPIController : ControllerBase
    {
        
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Video, long> _videoRepository;
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
        private readonly IRepository<StoreInfo, long> _storeInfoRepository;
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
        private readonly IOperationRecordService _operationRecordService;
        private readonly INewsService _newsService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IRepository<Lottery, long> _lotteryRepository;
        private readonly ISearchHelper _searchHelper;
        private readonly IConfiguration _configuration;
        private readonly IRepository<LotteryUser, long> _lotteryUserRepository;
        private readonly IRepository<LotteryAward, long> _lotteryAwardRepository;
        private readonly IRepository<LotteryPrize, long> _lotteryPrizeRepository;
        private readonly IRepository<RobotReply, long> _robotReplyRepository;
        private readonly IRepository<SearchCache, long> _searchCacheRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<SteamInforTableModel, long> _steamInforTableModelRepository;
        private readonly IRepository<OperationRecord, long> _operationRecordRepository;
        private readonly IRepository<RankUser, long> _rankUsersRepository;
        private readonly IRepository<EntryStaff, long> _entryStaffRepository;
        private readonly IRepository<EntryInformationType, long> _entryInformationTypeRepository;
        private readonly IRepository<BasicEntryInformation, long> _basicEntryInformationRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IChartService _chartService;
        private readonly ILogger<AdminAPIController> _logger;
        private readonly ISteamInforService _steamInforService;
        private readonly IEditRecordService _editRecordService;
        private readonly IVideoService _videoService;
        private readonly IRepository<PerfectionOverview, long> _perfectionOverviewRepository;

        public AdminAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository, IRepository<EntryStaff, long> entryStaffRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository, IVideoService videoService,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRankService rankService, 
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IWeiXinService weiXinService, IEditRecordService editRecordService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository, IPerfectionService perfectionService, IWebHostEnvironment webHostEnvironment,
         IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, IRepository<SearchCache, long> searchCacheRepository,
        IArticleService articleService, IUserService userService,  IExamineService examineService, IRepository<Rank, long> rankRepository, INewsService newsService, ISteamInforService steamInforService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository, IRepository<SteamInforTableModel, long> steamInforTableModelRepository,
        IVoteService voteService, IRepository<Vote, long> voteRepository, IRepository<SteamInfor, long> steamInforRepository, ILotteryService lotteryService, IRepository<RobotReply, long> robotReplyRepository,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IConfiguration configuration, IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository, ILogger<AdminAPIController> logger,
        IRepository<LotteryAward, long> lotteryAwardRepository, ISearchHelper searchHelper, IChartService chartService, IOperationRecordService operationRecordService, IRepository<PlayedGame, long> playedGameRepository,
        IRepository<LotteryPrize, long> lotteryPrizeRepository, IRepository<OperationRecord, long> operationRecordRepository, IRepository<RankUser, long> rankUsersRepository, IRepository<Video, long> videoRepository,
        IRepository<PerfectionOverview, long> perfectionOverviewRepository, IRepository<StoreInfo, long> storeInfoRepository, IRepository<EntryInformationType, long> entryInformationTypeRepository, IRepository<BasicEntryInformation, long> basicEntryInformationRepository)
        {
            
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _tagRepository = tagRepository;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _examineService = examineService;
            
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
            _weiXinService = weiXinService;
            _robotReplyRepository = robotReplyRepository;
            _searchCacheRepository = searchCacheRepository;
            _webHostEnvironment = webHostEnvironment;
            _chartService = chartService;
            _operationRecordService = operationRecordService;
            _playedGameRepository = playedGameRepository;
            _logger = logger;
            _steamInforTableModelRepository = steamInforTableModelRepository;
            _operationRecordRepository = operationRecordRepository;
            _rankUsersRepository = rankUsersRepository;
            _steamInforService = steamInforService;
            _entryStaffRepository = entryStaffRepository;
            _editRecordService = editRecordService;
            _videoService = videoService;
            _videoRepository = videoRepository;
            _perfectionOverviewRepository= perfectionOverviewRepository;
            _storeInfoRepository = storeInfoRepository;
            _entryInformationTypeRepository = entryInformationTypeRepository;
            _basicEntryInformationRepository = basicEntryInformationRepository;
        }

        /// <summary>
        /// 获取编辑用户视图模型 管理员模式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<EditUserViewModel>> EditUser(string id)
        {
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                PersonalSignature = user.PersonalSignature,
                MainPageContext = user.MainPageContext,
                Integral = user.DisplayIntegral,
                ContributionValue = user.DisplayContributionValue,
                MBgImageName = _appHelper.GetImagePath(user.MBgImage, ""),
                SBgImageName = _appHelper.GetImagePath(user.SBgImage, ""),
                PhotoName = _appHelper.GetImagePath(user.PhotoPath, ""),
                BackgroundName = _appHelper.GetImagePath(user.BackgroundImage, ""),
                CanComment = user.CanComment ?? true,
                Roles = new List<UserRolesModel>(),
                Birthday = user.Birthday,
                IsPassedVerification = user.IsPassedVerification
            };
           
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
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == model.Id);

            if (user == null)
            {
                return new Result { Error = "当前用户不存在", Successful = false };
            }

            user.Email = model.Email;
            user.UserName = model.UserName;
            user.PersonalSignature = model.PersonalSignature;
            user.MainPageContext = model.MainPageContext;
            user.Birthday = model.Birthday;
            user.PhotoPath = model.PhotoName;
            user.MBgImage = model.MBgImageName;
            user.SBgImage = model.SBgImageName;
            user.CanComment = model.CanComment;
            user.BackgroundImage = model.BackgroundName;
            user.IsPassedVerification = model.IsPassedVerification;

            //更新数据
            var result = await _userRepository.UpdateAsync(user);

            return new Result { Successful = true };
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

        /// <summary>
        /// 获取视频列表
        /// </summary>
        /// <param name="input">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListVideoAloneModel>>> GetVideoListAsync(VideosPagesInfor input)
        {
            var dtos = await _videoService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListMessageAloneModel>>> GetMessageListAsync(MessagesPagesInfor input)
        {
            var dtos = await _messageService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListErrorCountAloneModel>>> GetErrorCountListAsync(ErrorCountsPagesInfor input)
        {
            var dtos = await _errorCountService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListFavoriteFolderAloneModel>>> GetFavoriteFolderListAsync(FavoriteFoldersPagesInfor input)
        {
            var dtos = await _favoriteFolderService.GetPaginatedResult(input.Options, input.SearchModel, null);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListPeripheryAloneModel>>> GetPeripheryListAsync(PeripheriesPagesInfor input)
        {
            var dtos = await _peripheryService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListVoteAloneModel>>> GetVoteListAsync(VotesPagesInfor input)
        {
            var dtos = await _voteService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
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
            var model = new EditCarouselsViewModel();
            var carousels = await _carouselRepository.GetAll().OrderByDescending(s => s.Priority).ToListAsync();
            foreach (var item in carousels)
            {
                model.Carousels.Add(new CarouselModel
                {
                    Link = item.Link,
                    Priority = item.Priority,
                    Note = item.Note,
                    Image = _appHelper.GetImagePath(item.Image, ""),
                    Type=item.Type,
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
                    _ = await _carouselRepository.InsertAsync(new Carousel
                    {
                        Image = item.Image,
                        Link = item.Link,
                        Priority = item.Priority,
                        Note = item.Note,
                        Type=item.Type
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
                    Image = _appHelper.GetImagePath(item.Image, "app.png")
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
                    _ = await _friendLinkRepository.InsertAsync(new FriendLink
                    {
                        Image = item.Image,
                        Name = item.Name,
                        Link = item.Link,
                        Priority = item.Priority
                    });
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
                await _searchCacheRepository.GetAll().ExecuteDeleteAsync();
                await _searchHelper.DeleteDataOfSearchService();
                await _searchHelper.UpdateDataToSearchService(DateTime.Now, true);

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
                var types = new List<EntryInformationType>
                {
                    new EntryInformationType
                    {
                        Name = "原作",
                        Icon = "mdi-script-text",
                        Types = new EntryType[] { EntryType.Game }
                    },
                    new EntryInformationType
                    {
                        Name = "QQ群",
                        Icon = "mdi-qqchat",
                        Types = new EntryType[] { EntryType.Game, EntryType.ProductionGroup }
                    },
                    new EntryInformationType
                    {
                        Name = "性别",
                        Icon = "mdi-gender-male-female",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "身材数据",
                        Icon = "mdi-database",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "身材(主观)",
                        Icon = "mdi-human-handsup",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "生日",
                        Icon = "mdi-cake-variant",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "发色",
                        Icon = "mdi-face-man-profile",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "瞳色",
                        Icon = "mdi-eye",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "服饰",
                        Icon = "mdi-hanger",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "性格",
                        Icon = "mdi-account-outline",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "角色身份",
                        Icon = "mdi-account",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "血型",
                        Icon = "mdi-water",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "身高",
                        Icon = "mdi-human-male-height-variant",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "兴趣",
                        Icon = "mdi-heart-multiple",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "年龄",
                        Icon = "mdi-forest",
                        Types = new EntryType[] { EntryType.Role }
                    },
                    new EntryInformationType
                    {
                        Name = "姓名",
                        Icon = "mdi-card-account-details-star",
                        Types = new EntryType[] { EntryType.Staff }
                    }
                };

                foreach (var item in types)
                {
                    if(await _entryInformationTypeRepository.GetAll().AnyAsync(s=>s.Name==item.Name)==false)
                    {
                        await _entryInformationTypeRepository.InsertAsync(item);
                    }
                }

                await _basicEntryInformationRepository.GetAll().Where(s => s.DisplayName == "性别").ExecuteUpdateAsync(s => s.SetProperty(a => a.DisplayValue, b => b.DisplayValue == "None"? "保密" : b.DisplayValue == "Man" ? "男" : b.DisplayValue == "Women" ? "女" :  "其他"));

                return new Result { Successful = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行临时脚本失败");
                return new Result { Successful = false, Error = ex.Message + "\n" + ex.StackTrace + "\n" + ex.Source };
            }

        }


        /// <summary>
        /// 获取服务器动态数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ServerRealTimeOverviewModel>> GetServerRealTimeDataOverview()
        {
            return await Task.FromResult(SystemEnvironmentHelper.GetServerRealTimeDataOverview());
        }

        /// <summary>
        /// 获取服务器静态数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<ServerStaticOverviewModel>> GetServerStaticDataOverview()
        {
            return await Task.FromResult(SystemEnvironmentHelper.GetServerStaticDataOverview());
        }

        /// <summary>
        /// 获取用户数据概览
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<UserDataOverviewModel>> GetUserDataOverview()
        {
            var model = new UserDataOverviewModel();
            var now = DateTime.Now.ToCstTime().AddDays(-7);

            model.UserTotalNumber = await _userRepository.GetAll().CountAsync();
            model.UserVerifyEmailNumber = await _userRepository.GetAll().CountAsync(s => s.EmailConfirmed);
            model.UserSecondaryVerificationNumber = await _userRepository.GetAll().CountAsync(s => s.IsPassedVerification);
            model.UserActiveNumber = await _userRepository.GetAll().CountAsync(s => s.LastOnlineTime > now);

            return model;

        }

        /// <summary>
        /// 获取图表 管理员限定
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<LineChartModel>> GetLineChartAsync([FromQuery] LineChartType type, [FromQuery] long afterTime, [FromQuery] long beforeTime)
        {
            var after = afterTime.ToString().TransTime();
            var before = beforeTime.ToString().TransTime();

            return await _chartService.GetLineChartAsync(type, after, before);
        }


    }
}
