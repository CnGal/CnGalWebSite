using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Charts;
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
using CnGalWebSite.APIServer.Model;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Anniversaries;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
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
using System.Text;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/anniversaries/[action]")]
    public class AnniversariesAPIController : ControllerBase
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
        private readonly IRepository<RobotReply, long> _robotReplyRepository;
        private readonly IRepository<SearchCache, long> _searchCacheRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IChartService _chartService;

        public AnniversariesAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRankService rankService, IHistoryDataService historyDataService,
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IWeiXinService weiXinService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository, IPerfectionService perfectionService, IWebHostEnvironment webHostEnvironment,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, IRepository<SearchCache, long> searchCacheRepository,
        IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService, IRepository<Rank, long> rankRepository, INewsService newsService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository, IRepository<PlayedGame, long> playedGameRepository,
        IVoteService voteService, IRepository<Vote, long> voteRepository, IRepository<SteamInfor, long> steamInforRepository, ILotteryService lotteryService, IRepository<RobotReply, long> robotReplyRepository,
        IRepository<WeeklyNews, long> weeklyNewsRepository, IConfiguration configuration, IRepository<Lottery, long> lotteryRepository, IRepository<LotteryUser, long> lotteryUserRepository,
        IRepository<LotteryAward, long> lotteryAwardRepository, ISearchHelper searchHelper, IChartService chartService,
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
            _weiXinService = weiXinService;
            _robotReplyRepository = robotReplyRepository;
            _searchCacheRepository = searchCacheRepository;
            _webHostEnvironment = webHostEnvironment;
            _chartService = chartService;
            _playedGameRepository = playedGameRepository;
        }


        static readonly DateTime before = new DateTime(2022, 5, 31);
        static readonly DateTime after = new DateTime(2021, 5, 31);

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<JudgableGameViewModel>>> GetAllJudgableGamesAsync()
        {


            var games = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.PlayedGames)
                .Where(s => s.Type == EntryType.Game && s.PubulishTime != null && s.Id != 139 && s.Id != 3412 && s.Id != 3835 && s.IsHidden == false&& s.PubulishTime.Value.Date <= before.Date && s.PubulishTime.Value.Date >= after.Date)
                .ToListAsync();
            var model = new List<JudgableGameViewModel>();
            foreach (var item in games)
            {

                var temp = new JudgableGameViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Name : item.DisplayName,
                    MainImage = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    BriefIntroduction = item.BriefIntroduction,
                    LastEditTime = item.LastEditTime,
                    ReaderCount = item.ReaderCount,
                    CommentCount = item.CommentCount,
                    PublishTime = item.PubulishTime.Value
                };

                //处理评分信息
                if (item.PlayedGames != null && item.PlayedGames.Any())
                {
                    temp.LastScoreTime = item.PlayedGames.Max(s => s.LastEditTime);
                    temp.ScoreCount = item.PlayedGames.Count(s => s.IsScored && s.ShowPublicly);
                }

                model.Add(temp);
            }

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PlayedGameUserScoreRandomModel>>> GetRandomUserScoresAsync()
        {
            var games = await _playedGameRepository.GetAll().AsNoTracking()
                .Include(s => s.ApplicationUser)
                .Include(s=>s.Entry).ThenInclude(s=>s.Tags)
                .Where(s =>  s.Entry.PubulishTime != null && s.Entry.Id != 139 && s.Entry.Id != 3412 && s.Entry.Id != 3835 && s.Entry.IsHidden==false && s.Entry.PubulishTime.Value.Date <= before.Date && s.Entry.PubulishTime.Value.Date >= after.Date)
                .Where(s =>s.ShowPublicly && s.MusicSocre != 0 && s.PaintSocre != 0 && s.CVSocre != 0 && s.SystemSocre != 0 && s.ScriptSocre != 0 && s.TotalSocre != 0 && s.CVSocre != 0 && string.IsNullOrWhiteSpace(s.PlayImpressions)==false && s.PlayImpressions.Length > ToolHelper.MinValidPlayImpressionsLength)
                .ToListAsync();
            var model = new List<PlayedGameUserScoreRandomModel>();
            foreach (var item in games)
            {

                var temp = new PlayedGameUserScoreRandomModel
                {
                    Socres = new PlayedGameScoreModel
                    {
                        ScriptSocre = item.ScriptSocre,
                        ShowSocre = item.ShowSocre,
                        MusicSocre = item.MusicSocre,
                        PaintSocre = item.PaintSocre,
                        TotalSocre = item.TotalSocre,
                        SystemSocre = item.SystemSocre,
                        CVSocre = item.CVSocre,

                    },
                    LastEditTime = item.LastEditTime,
                    PlayImpressions = item.PlayImpressions,
                    User = await _userService.GetUserInforViewModel(item.ApplicationUser, true),
                    GameId = item.Entry.Id,
                    GameName = item.Entry.Name,
                    IsDubbing = !(item.Entry.Tags != null && item.Entry.Tags.Any(s => s.Name == "无配音"))
                };

                model.Add(temp);
            }

            return model;
        }
    }
}
