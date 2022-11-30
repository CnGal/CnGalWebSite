using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.News;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.DataModel.ViewModel.Search;
using Markdig;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Editor")]
    [ApiController]
    [Route("api/news/[action]")]
    public class NewsAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IUserService _userService;
        private readonly IAppHelper _appHelper;
        private readonly IArticleService _articleService;
        private readonly INewsService _newsService;
        private readonly IRepository<GameNews, long> _gameNewsRepository;
        private readonly IRepository<WeeklyNews, long> _weeklyNewsRepository;
        private readonly IRepository<WeiboUserInfor, long> _weiboUserInforRepository;


        public NewsAPIController(IRepository<WeiboUserInfor, long> weiboUserInforRepository, UserManager<ApplicationUser> userManager, IArticleService articleService, IUserService userService, INewsService newsService,
        IRepository<Article, long> articleRepository, IAppHelper appHelper, IRepository<GameNews, long> gameNewsRepository,
           IRepository<WeeklyNews, long> weeklyNewsRepository)
        {
            _userManager = userManager;
            _appHelper = appHelper;
            _articleRepository = articleRepository;
            _userService = userService;
            _articleService = articleService;
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
                await _gameNewsRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.State, b => GameNewsState.Ignore));
                return new Result { Successful = true };

            }

            var news = await _gameNewsRepository.GetAll().Include(s => s.Article).Where(s => model.Ids.Contains(s.Id)).ToListAsync();
            foreach (var item in news)
            {
                if (item.Article != null)
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

            if (gameNews == null)
            {
                return NotFound("未找到该动态");
            }

            //查找作者关联词条
            var author = await _weiboUserInforRepository.GetAll().Include(s => s.Entry).FirstOrDefaultAsync(s => s.WeiboName == gameNews.Author);

            var model = new EditGameNewsModel
            {
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
                WeiboId = (author == null || author?.WeiboId == 0) ? "" : author.WeiboId.ToString(),
                ArticleId = gameNews.ArticleId,
            };

            foreach (var item in gameNews.Entries)
            {
                model.Entries.Add(new DataModel.ViewModel.RelevancesModel
                {
                    DisplayName = item.EntryName
                });
            }

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> EditGameNewsAsync(EditGameNewsModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Author) || string.IsNullOrWhiteSpace(model.Title))
            {
                return new Result { Successful = false, Error = "作者名称或标题不能为空" };
            }

            var gameNews = await _gameNewsRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == model.Id);

            if (gameNews == null)
            {
                return new Result { Successful = false, Error = "未找到该动态" };
            }


            //查找作者关联词条
            var author = await _weiboUserInforRepository.GetAll().AsNoTracking().Include(s => s.Entry).FirstOrDefaultAsync(s => s.WeiboName == gameNews.Author);

            //查看是否修正了作者信息
            if (author == null && string.IsNullOrWhiteSpace(model.AuthorEntryName) == false && string.IsNullOrWhiteSpace(model.WeiboId) == false)
            {
                try
                {
                    await _newsService.AddWeiboUserInfor(model.AuthorEntryName, long.Parse(model.WeiboId));

                }
                catch
                {
                    return new Result { Successful = false, Error = "尝试获取作者信息失败" };
                }
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
            foreach (var item in model.Entries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false))
            {
                gameNews.Entries.Add(new GameNewsRelatedEntry
                {
                    EntryName = item.DisplayName
                });
            }

            //简单的将名称之类的信息同步到对应文章
            if (gameNews.State == GameNewsState.Publish)
            {
                var article = await _articleRepository.FirstOrDefaultAsync(s => s.Id == gameNews.ArticleId);

                article.OriginalAuthor = model.Author;
                article.OriginalLink = model.Link;
                article.DisplayName = model.Title;
                article.Name = model.Title;
                article.BriefIntroduction = model.BriefIntroduction;
                article.MainPage = model.MainPage;
                article.MainPicture = model.MainPicture;
                article.NewsType = model.NewsType;
                article.PubishTime = model.PublishTime;
                article.RealNewsTime = model.PublishTime;

                await _articleRepository.UpdateAsync(article);
            }

            await _gameNewsRepository.UpdateAsync(gameNews);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddCustomNewsAsync(EditGameNewsModel model)
        {

            if (string.IsNullOrWhiteSpace(model.Author) || string.IsNullOrWhiteSpace(model.Title))
            {
                return new Result { Successful = false, Error = "作者名称或标题不能为空" };
            }

            var gameNews = new GameNews
            {
                Author = model.Author,
                BriefIntroduction = model.BriefIntroduction,
                Link = model.Link,
                MainPage = model.MainPage,
                MainPicture = model.MainPicture,
                NewsType = model.NewsType,
                PublishTime = model.PublishTime,
                Title = model.Title,
                Type = model.Type,

                RSS = new OriginalRSS
                {
                    Author = model.Author,
                    Description = model.MainPage,
                    Link = model.Link,
                    PublishTime = model.PublishTime,
                    Title = model.Title,
                    Type = OriginalRSSType.Custom
                }
            };

            gameNews.Entries.Clear();
            foreach (var item in model.Entries.Where(s => string.IsNullOrWhiteSpace(s.DisplayName) == false))
            {
                gameNews.Entries.Add(new GameNewsRelatedEntry
                {
                    EntryName = item.DisplayName
                });
            }

            //查找作者关联词条
            var author = await _weiboUserInforRepository.GetAll().AsNoTracking().Include(s => s.Entry).FirstOrDefaultAsync(s => s.WeiboName == gameNews.Author);

            //查看是否修正了作者信息
            if (author == null && string.IsNullOrWhiteSpace(model.AuthorEntryName) == false && string.IsNullOrWhiteSpace(model.WeiboId) == false)
            {
                try
                {
                    await _newsService.AddWeiboUserInfor(model.AuthorEntryName, long.Parse(model.WeiboId));

                }
                catch
                {
                    return new Result { Successful = false, Error = "尝试获取作者信息失败" };
                }
            }

            await _gameNewsRepository.InsertAsync(gameNews);

            return new Result { Successful = true };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> PublishGameNewsAsync(long id)
        {
            try
            {
                var gameNews = await _gameNewsRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == id);

                if (gameNews == null)
                {
                    return new Result { Successful = false, Error = "未找到该动态" };
                }

                await _newsService.PublishNews(gameNews);

                return new Result { Successful = true };
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleViewModel>> GetGameNewsPreviewAsync(long id)
        {
            var gameNews = await _gameNewsRepository.GetAll().Include(s => s.Entries).FirstOrDefaultAsync(s => s.Id == id);
            if (gameNews == null)
            {
                return NotFound("未找到该动态");
            }
            //生成文章
            Article article = null;
            try
            {
                article = await _newsService.GameNewsToArticle(gameNews);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            article.CreateUser = await _userManager.FindByIdAsync(article.CreateUserId);

            //正常流程初始化
            var model = new ArticleViewModel
            {
                Id = article.Id,
                Name = article.DisplayName ?? article.Name,
                Type = article.Type,
                MainPage = article.MainPage,
                PubishTime = article.RealNewsTime ?? article.PubishTime,
                OriginalLink = article.OriginalLink,
                OriginalAuthor = article.OriginalAuthor,
                BriefIntroduction = article.BriefIntroduction,
            };
            //初始化图片
            model.MainPicture = _appHelper.GetImagePath(article.MainPicture, "app.png");

            //初始化主页Html代码
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.MainPage = Markdown.ToHtml(model.MainPage ?? "", pipeline);

            //走动态的作者初始化流程
            var infor = await _articleService.GetNewsModelAsync(article);

            model.UserInfor = new DataModel.ViewModel.Space.UserInforViewModel
            {
                Name = infor.GroupName,
                PhotoPath = infor.Image
            };

            return model;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EditWeeklyNewsModel>> EditWeeklyNewsAsync(long id)
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s => s.News).FirstOrDefaultAsync(s => s.Id == id);

            if (weeklyNews == null)
            {
                return NotFound("未找到该周报");
            }

            var model = new EditWeeklyNewsModel
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
                ArticleId = weeklyNews.ArticleId,
            };

            //选中的动态
            //获取在这个星期的所有动态

            var news = await _gameNewsRepository.GetAll().Where(s => model.CreateTime.AddDays(7) > s.PublishTime && model.CreateTime.AddDays(-7) < s.PublishTime).ToListAsync();
            news = news.Where(s => s.PublishTime.IsInSameWeek(model.CreateTime)).ToList();

            foreach (var item in news)
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
                article.Name = model.Title;
                article.BriefIntroduction = model.BriefIntroduction;
                article.MainPage = _newsService.GenerateRealWeeklyNewsMainPage(weeklyNews);
                article.MainPicture = model.MainPicture;

                await _articleRepository.UpdateAsync(article);
            }

            await _weeklyNewsRepository.UpdateAsync(weeklyNews);

            return new Result { Successful = true };
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> PublishWeelyNewsAsync(long id)
        {
            try
            {
                var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s => s.News).FirstOrDefaultAsync(s => s.Id == id);

                if (weeklyNews == null)
                {
                    return new Result { Successful = false, Error = "未找到该周报" };
                }

                await _newsService.PublishWeeklyNews(weeklyNews);

                return new Result { Successful = true };
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> ResetWeelyNewsAsync(long id)
        {
            try
            {
                var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s => s.News).FirstOrDefaultAsync(s => s.Id == id);

                if (weeklyNews == null)
                {
                    return new Result { Successful = false, Error = "未找到该周报" };
                }

                await _newsService.ResetWeeklyNews(weeklyNews);

                await _weeklyNewsRepository.UpdateAsync(weeklyNews);

                return new Result { Successful = true };
            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleViewModel>> GetWeelyNewsPreviewAsync(long id)
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s => s.News).FirstOrDefaultAsync(s => s.Id == id);

            if (weeklyNews == null)
            {
                return NotFound("未找到该周报");
            }

            //生成文章
            Article article = null;
            try
            {
                article = await _newsService.WeeklyNewsToArticle(weeklyNews);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            article.CreateUser = await _userManager.FindByIdAsync(article.CreateUserId);

            //正常流程初始化
            var model = new ArticleViewModel
            {
                Id = article.Id,
                Name = article.DisplayName ?? article.Name,
                Type = article.Type,
                MainPage = article.MainPage,
                PubishTime = article.CreateTime,
                OriginalLink = article.OriginalLink,
                OriginalAuthor = article.OriginalAuthor,
                BriefIntroduction = article.BriefIntroduction,
                UserInfor = await _userService.GetUserInforViewModel(article.CreateUser)
            };

            //初始化图片
            model.MainPicture = _appHelper.GetImagePath(article.MainPicture, "app.png");

            //初始化主页Html代码
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().Build();
            model.MainPage = Markdown.ToHtml(model.MainPage ?? "", pipeline);

            return model;

        }

        [HttpGet]
        public async Task<ActionResult<ListGameNewsInforViewModel>> ListGameNewsAsync()
        {
            var model = new ListGameNewsInforViewModel
            {
                All = await _gameNewsRepository.LongCountAsync() + await _weeklyNewsRepository.LongCountAsync(),
                GameNews = await _gameNewsRepository.LongCountAsync(),
                PublishedNews = await _gameNewsRepository.LongCountAsync(s => s.State == GameNewsState.Publish),
                DeletedNews = await _gameNewsRepository.LongCountAsync(s => s.State == GameNewsState.Ignore),
                WeelyNews = await _weeklyNewsRepository.LongCountAsync()
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListGameNewAloneModel>>> GetGameNewListAsync(GameNewsPagesInfor input)
        {
            var dtos = await _newsService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListWeeklyNewAloneModel>>> GetWeeklyNewListAsync(WeeklyNewsPagesInfor input)
        {
            var dtos = await _newsService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddWeiboNewsAsync(AddWeiboNewsModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Link))
            {
                return new Result { Successful = false, Error = "链接不能为空" };
            }
            //尝试拆分
            var text = model.Link.Split('/');
            if (text.Length < 2)
            {
                return new Result { Successful = false, Error = "链接无效" };
            }
            if (long.TryParse(text[3], out var id) == false)
            {
                return new Result { Successful = false, Error = "链接无效，无法识别Id" };
            }
            if (id <= 0)
            {
                return new Result { Successful = false, Error = "链接无效，识别到的Id不符合条件" };
            }

            var keyword = text[4];

            //查找是否已经录入
            if (await _gameNewsRepository.GetAll().Include(s => s.RSS).AnyAsync(s => (s.RSS.Link == model.Link || s.Link == model.Link) && s.Title != "已删除"))
            {
                return new Result { Successful = false, Error = "该微博动态已存在" };
            }

            try
            {
                await _newsService.AddGameMewsFromWeibo(id, keyword);

            }
            catch (Exception ex)
            {
                return new Result { Successful = false, Error = ex.Message };
            }

            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ArticleInforTipViewModel>>> GetWeeklyNewsOverviewAsync()
        {
            var weeklyNews = await _weeklyNewsRepository.GetAll().Include(s => s.Article).Where(s => s.State == GameNewsState.Publish).ToListAsync();

            var model = new List<ArticleInforTipViewModel>();
            foreach (var item in weeklyNews.OrderByDescending(s => s.PublishTime).Take(8))
            {
                var temp = _appHelper.GetArticleInforTipViewModel(item.Article);
                temp.DisplayName = temp.DisplayName.Replace("CnGal每周速报（", "").Replace("）", "");
                model.Add(temp);
            }

            return model;
        }

    }
}
