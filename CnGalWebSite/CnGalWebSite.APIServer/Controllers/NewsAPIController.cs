using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.ErrorCounts;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CnGalWebSite.APIServer.Application.ElasticSearches;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.DataModel.ViewModel.News;
using Elasticsearch.Net;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/news/[action]")]
    public class NewsAPIController : ControllerBase
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
        private readonly IElasticsearchBaseService<Entry> _entryElasticsearchBaseService;
        private readonly IElasticsearchBaseService<Article> _articleElasticsearchBaseService;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly INewsService _newsService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IRepository<WeiboUserInfor, long> _weiboUserInforRepository;


        public NewsAPIController(IRepository<UserOnlineInfor, long> userOnlineInforRepository, IRepository<UserFile, int> userFileRepository, IRepository<FavoriteObject, long> favoriteObjectRepository,
        IFileService fileService, IRepository<SignInDay, long> signInDayRepository, IRepository<ErrorCount, long> errorCountRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository,
        IRepository<ThumbsUp, long> thumbsUpRepository, IRepository<Disambig, int> disambigRepository, IRepository<BackUpArchive, long> backUpArchiveRepository, IRankService rankService, IRepository<WeiboUserInfor, long> weiboUserInforRepository,
        IRepository<ApplicationUser, string> userRepository, IMessageService messageService, ICommentService commentService, IRepository<Comment, long> commentRepository, IElasticsearchService elasticsearchService,
        IRepository<Message, long> messageRepository, IErrorCountService errorCountService, IRepository<FavoriteFolder, long> favoriteFolderRepository, IPerfectionService perfectionService, IElasticsearchBaseService<Article> articleElasticsearchBaseService,
        UserManager<ApplicationUser> userManager, IRepository<FriendLink, int> friendLinkRepository, IRepository<Carousel, int> carouselRepositor, IEntryService entryService, IElasticsearchBaseService<Entry> entryElasticsearchBaseService,
        IArticleService articleService, IUserService userService, RoleManager<IdentityRole> roleManager, IExamineService examineService, IRepository<Rank, long> rankRepository, INewsService newsService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IFavoriteFolderService favoriteFolderService, IRepository<Periphery, long> peripheryRepository,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IPeripheryService peripheryService, IRepository<GameNews, long> gameNewsRepository,
           IRepository<WeeklyNews, long> weeklyNewsRepository )
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
            _weiboUserInforRepository = weiboUserInforRepository;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> IgnoreGameNewsAsync(IgnoreGameNewsModel model)
        {
            if (model.IsIgnore == true)
            {
                await _gameNewsRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.State, b => GameNewsState.Ignore).ExecuteAsync();
                return new Result { Successful = true };

            }

            var news = await _gameNewsRepository.GetAll().Include(s=>s.Article).Where(s => model.Ids.Contains(s.Id)).ToListAsync();
            foreach(var item in news)
            {
                if(item.Article!=null)
                {
                    item.State = GameNewsState.Publish;
                }
                else
                {
                    item.State = GameNewsState.Edit;
                }
                await _gameNewsRepository.UpdateAsync(item);
            }

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditGameNewsModel>> EditGameNewsAsync(long id)
        {
            var gameNews = await _gameNewsRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == id);

            if(gameNews == null)
            {
                return NotFound("未找到该动态");
            }

            //查找作者关联词条
            var author = await _weiboUserInforRepository.GetAll().Include(s => s.Entry).FirstOrDefaultAsync(s => s.WeiboName == gameNews.Author);

            EditGameNewsModel model = new EditGameNewsModel
            {
                Entries = gameNews.Entries.Select(s=>s.EntryName).ToList(),
                AuthorEntryName = author?.Entry?.Name ?? "",
                Author = gameNews.Author,
                State = gameNews.State,
                BriefIntroduction = gameNews.BriefIntroduction,
                Id = gameNews.Id,
                Link = gameNews.Link,
                MainPage = gameNews.MainPage,
                MainPicture = gameNews.MainPicture,
                NewsType = gameNews.NewsType,
                PublishTime = gameNews.PublishTime,
                Title = gameNews.Title,
                Type = gameNews.Type,
                WeiboId = author?.WeiboId ?? 0
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditGameNewsAsync(EditGameNewsModel model)
        {

            if(string.IsNullOrWhiteSpace(model.Author)||string.IsNullOrWhiteSpace(model.Title))
            {
                return new Result { Successful = false ,Error= "作者名称或标题不能为空" };
            }

            var gameNews = await _gameNewsRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == model.Id);

            if (gameNews == null)
            {
                return new Result { Successful = false, Error = "未找到该动态" };
            }


            //查找作者关联词条
            var author = await _weiboUserInforRepository.GetAll().AsNoTracking().Include(s => s.Entry).FirstOrDefaultAsync(s => s.WeiboName == gameNews.Author);

            //查看是否修正了作者信息
            if (author == null && string.IsNullOrWhiteSpace(model.AuthorEntryName)==false && model.WeiboId != 0)
            {
                await _newsService.AddWeiboUserInfor(model.AuthorEntryName, model.WeiboId);
            }

            //更新数据
            gameNews.Author = model.Author;
            gameNews.BriefIntroduction = model.BriefIntroduction;
            gameNews.Link = model.Link;
            gameNews.MainPage = model.MainPage;
            gameNews.MainPicture = model.MainPicture;
            gameNews.NewsType = model.NewsType;
            gameNews.PublishTime = model.PublishTime;
            gameNews.Title = model.Title;
            gameNews.Type = model.Type;

            gameNews.Entries.Clear();
            foreach(var item in model.Entries)
            {
                gameNews.Entries.Add(new GameNewsRelatedEntry
                {
                    EntryName = item
                });
            }

            //简单的将名称之类的信息同步到对应文章
            if (gameNews.State==GameNewsState.Publish)
            {
                var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == gameNews.ArticleId);

                article.OriginalAuthor = model.Author;
                article.OriginalLink=model.Link;
                article.DisplayName = model.Title;
                article.BriefIntroduction = model.BriefIntroduction;
                article.MainPage = model.MainPage;
                article.MainPicture=model.MainPicture;

                await _articleRepository.UpdateAsync(article);
            }

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> PublishGameNewsAsync(long id)
        {
            var gameNews = await _gameNewsRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == id);

            if (gameNews == null)
            {
                return new Result { Successful = false, Error = "未找到该动态" };
            }

            await _newsService.PublishNews(gameNews);

            return new Result { Successful = true };
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<EditWeeklyNewsModel>> EditWeeklyNewsAsync(long id)
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s=>s.News).FirstOrDefaultAsync(s => s.Id == id);

            if (weeklyNews == null)
            {
                return NotFound("未找到该周报");
            }

            EditWeeklyNewsModel model = new EditWeeklyNewsModel
            {
                State = weeklyNews.State,
                BriefIntroduction = weeklyNews.BriefIntroduction,
                Id = weeklyNews.Id,
                MainPage = weeklyNews.MainPage,
                MainPicture = weeklyNews.MainPicture,
                PublishTime = weeklyNews.PublishTime,
                Title = weeklyNews.Title,
                Type = weeklyNews.Type,
                CreateTime = weeklyNews.CreateTime,
            };

            //选中的动态
            //获取在这个星期的所有动态
            DateTime dateTime = DateTime.Now.ToCstTime();
            var news = await _gameNewsRepository.GetAll().Where(s => dateTime.AddDays(-14) < s.PublishTime).ToListAsync();
            news = news.Where(s => s.PublishTime.IsInSameWeek(dateTime)).ToList();

           foreach(var item in news)
            {
                model.News.Add(new WeeklyNewsRelatedNewsEditModel
                {
                    IsSelected = weeklyNews.News.Any(s => s.Id == item.Id),
                    NewsId = item.Id,
                    NewsTitle = item.Title,
                });
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditWeeklyNewsAsync(EditWeeklyNewsModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return new Result { Successful = false, Error = "标题不能为空" };
            }

            var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s => s.News).FirstOrDefaultAsync(s => s.Id == model.Id);

            if (weeklyNews == null)
            {
                return new Result { Successful = false, Error = "未找到该周报" };
            }



            //更新数据
            weeklyNews.BriefIntroduction = model.BriefIntroduction;
            weeklyNews.MainPage = model.MainPage;
            weeklyNews.CreateTime = model.CreateTime;
            weeklyNews.MainPicture = model.MainPicture;
            weeklyNews.PublishTime = model.PublishTime;
            weeklyNews.Title = model.Title;
            weeklyNews.Type = model.Type;

            //选择动态
            weeklyNews.News.Clear();
            foreach (var item in model.News.Where(s => s.IsSelected))
            {
                weeklyNews.News.Add(await _gameNewsRepository.FirstOrDefaultAsync(s => s.Id == item.NewsId));
            }

            //简单的将名称之类的信息同步到对应文章
            if (weeklyNews.State == GameNewsState.Publish)
            {
                var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == weeklyNews.ArticleId);

                article.DisplayName = model.Title;
                article.BriefIntroduction = model.BriefIntroduction;
                article.MainPage = model.MainPage;
                article.MainPicture = model.MainPicture;

                await _articleRepository.UpdateAsync(article);
            }

            return new Result { Successful = true };
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> PublishWeelyNewsAsync(long id)
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().FirstOrDefaultAsync(s => s.Id == id);

            if (weeklyNews == null)
            {
                return new Result { Successful = false, Error = "未找到该周报" };
            }

            await _newsService.PublishWeeklyNews(weeklyNews);

            return new Result { Successful = true };
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> ResetWeelyNewsAsync(long id)
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().FirstOrDefaultAsync(s => s.Id == id);

            if (weeklyNews == null)
            {
                return new Result { Successful = false, Error = "未找到该周报" };
            }

            _newsService.ResetWeeklyNews(weeklyNews);

            await _weeklyNewsRepository.UpdateAsync(weeklyNews);

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WeelyNewsPreviewModel>> GetWeelyNewsPreviewAsync(long id)
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().FirstOrDefaultAsync(s => s.Id == id);

            if (weeklyNews == null)
            {
                return NotFound();
            }

            var model = new WeelyNewsPreviewModel
            {
                Id = weeklyNews.Id,
                MainPage = _newsService.GenerateRealWeeklyNewsMainPage(weeklyNews),
                Title = weeklyNews.Title,
            };

            return model;
        }
    }
}
