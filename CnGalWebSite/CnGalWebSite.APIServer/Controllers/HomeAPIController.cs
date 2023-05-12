using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Home;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/home/[action]")]
    public class HomeAPIController : ControllerBase
    {
        private readonly ISearchHelper _searchHelper;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<SteamInfor, long> _steamInforRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IHomeService _homeService;
        private readonly IExamineService _examineService;
        private readonly IAppHelper _appHelper;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<HomeAPIController> _logger;
        private readonly ISteamInforService _steamInforService;

        public HomeAPIController(ISearchHelper searchHelper, IAppHelper appHelper, IRepository<Article, long> articleRepository, IHostApplicationLifetime applicationLifetime, ILogger<HomeAPIController> logger,
        IRepository<Entry, int> entryRepository, IHomeService homeService, IExamineService examineService, IRepository<Examine, long> examineRepository, ISteamInforService steamInforService, IRepository<Tag, int> tagRepository,
        IRepository<SteamInfor, long> steamInforRepository, IRepository<Comment, long> commentRepository)
        {
            _searchHelper = searchHelper;
            _entryRepository = entryRepository;
            _homeService = homeService;
            _examineService = examineService;
            _appHelper = appHelper;
            _examineRepository = examineRepository;
            _articleRepository = articleRepository;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _steamInforService = steamInforService;
            _tagRepository = tagRepository;
            _steamInforRepository = steamInforRepository;
            _commentRepository = commentRepository;
        }

        /// <summary>
        /// 获取即将发售游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<PublishedGameItemModel>>> ListPublishedGames()
        {
            return await _homeService.ListPublishedGames();
        }
        /// <summary>
        /// 获取近期编辑的游戏或制作组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<RecentlyEditedGameItemModel>>> ListRecentlyEditedGames()
        {
            return await _homeService.ListRecentlyEditedGames();

        }
        /// <summary>
        /// 获取近期发售的游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<UpcomingGameItemModel>>> ListUpcomingGames()
        {
            return await _homeService.ListUpcomingGames();

        }

        /// <summary>
        /// 获取友情链接 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<FriendLinkItemModel>>> ListFriendLinks()
        {
            return await _homeService.ListFriendLinks();

        }

        /// <summary>
        /// 获取通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<AnnouncementItemModel>>> ListAnnouncements()
        {
            return await _homeService.ListAnnouncements();

        }
        /// <summary>
        /// 获取最近发布的文章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<LatastArticleItemModel>>> ListLatastArticles()
        {
            return await _homeService.ListLatastArticles();

        }
        /// <summary>
        /// 获取最近发布的视频
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<LatastVideoItemModel>>> ListLatastVideoes()
        {
            return await _homeService.ListLatastVideoes();

        }
        /// <summary>
        /// 获取最近发布的动态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeNewsAloneViewModel>>> GetHomeNewsViewAsync()
        {
            return await _homeService.GetHomeNewsViewAsync();

        }
        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CarouselViewModel>>> GetHomeCarouselsViewAsync()
        {
            try
            {
                return await _homeService.GetHomeCarouselsViewAsync();
            }
            catch
            {
                //轮播图获取不到 代表数据库有问题 重启
                _applicationLifetime.StopApplication();
                return NotFound();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Types"></param>
        /// <param name="Times">时间戳 格式 13位 xxx-xxx</param>
        /// <param name="Text"></param>
        /// <param name="Sort"></param>
        /// <param name="Page"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<SearchViewModel>> SearchAsync([FromQuery] string[] Types, [FromQuery] string[] Times, [FromQuery] string Text, [FromQuery] string Sort, [FromQuery] int Page)
        {
            try
            {
                return new SearchViewModel
                {
                    pagedResultDto = await _searchHelper.QueryAsync(SearchInputModel.Parse(Types, Times, Text, Sort, Page))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取搜索结果失败");
                return NotFound(ex.Message);
            }
        }
        /// <summary>
        /// 获取搜索提示
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetSearchTipListAsync()
        {
            return await _entryRepository.GetAll().Where(s => s.IsHidden != true).AsNoTracking().Select(s => s.Name).Where(s => string.IsNullOrWhiteSpace(s) == false).ToArrayAsync();
        }

        [HttpGet]
        public async Task<ActionResult<List<DocumentViewModel>>> GetDocumentsAsync()
        {

            var model = new List<DocumentViewModel>
                {
                    new DocumentViewModel
                    {
                        Icon = "mdi-information-outline ",
                        Title = "关于我们",
                        Id=99999,
                        Children = new List<DocumentViewModel>
                        {
                            new DocumentViewModel
                            {
                                Title = "概述",
                                Id = 767,
                            },
                            new DocumentViewModel
                            {
                                Title = "组织架构",
                                Id = 150,
                            },
                            new DocumentViewModel
                            {
                                Title = "隐私政策",
                                Id = 225,
                            }
                        }
                    }
                };


            //更新日志
            var articles = await _articleRepository.GetAll().AsNoTracking()
                .Where(s => s.Type == ArticleType.Notice && s.Name.Contains("更新") && s.Id != 766)
                .Select(s => new DocumentViewModel { Id = s.Id, Title = s.DisplayName.Replace("网站", "") })
                .ToListAsync();
            var tempDocument = new DocumentViewModel
            {
                Icon = "mdi-cloud-upload-outline ",
                Id = 99998,
                Title = "更新日志",
                Children = articles.OrderByDescending(s => s.Id).ToList()
            };
            tempDocument.Children.Insert(0, new DocumentViewModel
            {
                Title = "概览",
                Id = 766,
            });
            model.Add(tempDocument);


            //公告
            articles = await _articleRepository.GetAll().AsNoTracking()
              .Where(s => s.Type == ArticleType.Notice && s.Name.Contains("更新") == false && s.Id != 150 && s.Id != 225 && s.Id != 767 && s.Id != 766)
              .Select(s => new DocumentViewModel { Id = s.Id, Title = s.DisplayName })
              .ToListAsync();

            model.Add(new DocumentViewModel
            {
                Icon = "mdi-clipboard-check-multiple-outline ",
                Id = 99997,
                Title = "公告",
                Children = articles.OrderByDescending(s => s.Id).ToList()
            });

            //周报

            articles = await _articleRepository.GetAll().AsNoTracking()
             .Where(s => s.Type == ArticleType.News && s.Name.Contains("CnGal每周速报"))
             .Select(s => new DocumentViewModel { Id = s.Id, Title = s.Name.Replace("CnGal每周速报（", "").Replace("）", "") })
             .ToListAsync();

            model.Add(new DocumentViewModel
            {
                Icon = "mdi-newspaper-variant ",
                Id = 99996,
                Title = "CnGal每周速报",
                Children = articles.OrderByDescending(s => s.Id).ToList()
            });

            return model;
        }

        [HttpPost]
        public async Task<List<PersonalRecommendModel>> GetPersonalizedRecommendations(IEnumerable<int> ids)
        {
            var model =new List<PersonalRecommendModel>();

            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                //.Include(s => s.Pictures)
                //.Where(s=>s.Pictures.Any())
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Type == EntryType.Game && ids.Contains(s.Id) == false)
                .Select(s => s.Id)
                .ToListAsync();

            entryIds = entryIds.Random().Take(10).ToList() ;

            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Releases)
                .Include(s => s.Pictures)
                .Include(s=>s.Tags)
                .Where(s => entryIds.Contains(s.Id))
                .ToListAsync();

            //随机
            var random = new Random();

            //图墙
            if(entries.Count(s=>s.Pictures.Any())>=5)
            {
               
                var imageEntries = new List<Entry>();

                foreach(var item in entries.Where(s => s.Pictures.Any()).Take(5))
                {
                    imageEntries.Add(item);
                }

                var temp = new PersonalRecommendModel
                {
                    Id = imageEntries.First().Id,
                    DisplayType = PersonalRecommendDisplayType.ImageGames
                };

                foreach (var item in imageEntries)
                {
                    entries.Remove(item);

                    temp.ImageCards.Add(new PersonalRecommendImageCardModel
                    {
                        Id = item.Id,
                        Name=item.DisplayName,
                        Image = item.Pictures.ToList().Random().OrderBy(s => s.Priority).FirstOrDefault()?.Url
                    });
                }

                model.Add(temp);
            }

            foreach (var item in entries)
            {


                var randomNum = random.Next(0, 2);
                if (/*randomNum == 0&&*/item.Pictures.Count > 3)
                {
                    var temp = new PersonalRecommendModel
                    {
                        Id = item.Id,
                        BriefIntroduction = item.BriefIntroduction,
                        DisplayType = PersonalRecommendDisplayType.Gallery,
                        MainPicture = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName,
                        Images = item.Pictures.Select(s => s.Url).ToList(),
                        Tags = item.Tags.Where(s => s.Name.Contains("字幕") == false && s.Name.Contains("语音") == false).Select(s => s.Name).ToList()
                    };
                    if (string.IsNullOrWhiteSpace(item.MainPicture) == false)
                    {
                        temp.Images.Insert(0,item.MainPicture);
                    }

                    model.Add(temp);

                    var release = item.Releases.FirstOrDefault(s => s.PublishPlatformType == PublishPlatformType.Steam && string.IsNullOrWhiteSpace(s.Link) == false);
                    if (release != null)
                    {
                        temp.SteamId = release.Link;
                    }
                }
                else
                {
                    //纯文本

                    var temp = new PersonalRecommendModel
                    {
                        Id = item.Id,
                        BriefIntroduction = item.BriefIntroduction,
                        DisplayType = PersonalRecommendDisplayType.PlainText,
                        MainPicture = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                        Name = item.DisplayName
                    };

                    model.Add(temp);

                    var release = item.Releases.OrderBy(s => s.Time).FirstOrDefault(s => s.Time != null && s.Type == GameReleaseType.Official);

                    if (release != null)
                    {
                        var infor = new GameReleaseViewModel
                        {
                            Engine = release.Engine,
                            GamePlatformTypes = release.GamePlatformTypes,
                            Link = release.Link,
                            Name = release.Name,
                            PublishPlatformName = release.PublishPlatformName,
                            PublishPlatformType = release.PublishPlatformType,
                            Time = release.Time,
                            TimeNote = release.TimeNote,
                            Type = release.Type,
                        };
                        if (release.PublishPlatformType == PublishPlatformType.Steam && int.TryParse(release.Link, out int steamId))
                        {
                            infor.StoreInfor = await _steamInforService.GetSteamInforAsync(steamId, item.Id);
                        }

                        temp.Release = infor;
                    }
                }
            }

            return model;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<FreeGameItemModel>>> ListFreeGames()
        {
            var entryIds = await _tagRepository.GetAll().AsNoTracking()
                .Include(s => s.Entries)
                .Where(s => s.Name == "免费")
                .Select(s => s.Entries.Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToList())
                .FirstOrDefaultAsync();

            entryIds = entryIds.ToList().Random().Take(16).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Tags)
                .Where(s => entryIds.Contains(s.Id)).ToListAsync();

            var model = new List<FreeGameItemModel>();
            foreach (var item in entries)
            {
                model.Add(new FreeGameItemModel
                {
                    Image = _appHelper.GetImagePath(item.MainPicture, "app.png"),
                    Name = item.DisplayName,
                    Url = "entries/index/" + item.Id,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    BriefIntroduction = item.BriefIntroduction,
                    Tags = item.Tags.Where(s => s.Name.Contains("字幕") == false && s.Name.Contains("语音") == false && s.Name.Contains("免费") == false && s.Name.Contains("界面") == false).Select(s => s.Name).ToList()
                });
            }

            return model;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<DiscountGameItemModel>>> ListDiscountGames()
        {
            var entryIds = await _steamInforRepository.GetAll().Include(s => s.Entry).Where(s => s.CutNow > 0 && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false).Select(s => s.Id).ToListAsync();
            entryIds = entryIds.ToList().Random().Take(16).ToList();

            var games = await _steamInforRepository.GetAll().Include(s => s.Entry).AsNoTracking().Where(s => entryIds.Contains(s.Id)).ToListAsync();

            var model = new List<DiscountGameItemModel>();
            foreach (var item in games)
            {
                var temp = new DiscountGameItemModel
                {
                    Image = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    Name = item.Entry.DisplayName,
                    Url = "entries/index/" + item.Entry.Id,
                    CommentCount = item.Entry.CommentCount,
                    ReadCount = item.Entry.ReaderCount,
                    BriefIntroduction = item.Entry.BriefIntroduction,
                    Price = item.PriceNow * 0.01,
                    Cut = item.CutNow
                };

                model.Add(temp);
            }

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<EvaluationItemModel>>> ListEvaluations()
        {
            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                .Include(s=>s.Pictures)
                .Where(s => s.Articles.Count >= 6 && s.WebsiteAddInfor != null && s.WebsiteAddInfor.Images.Any())
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .Select(s=>s.Id)
                .ToListAsync();

            entryIds = entryIds.ToList().Random().Take(4).ToList();


            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Articles).ThenInclude(s => s.CreateUser)
                .Include(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                .Include(s => s.Pictures)
                .Where(s => entryIds.Contains(s.Id))
                .ToListAsync();

            var model = new List<EvaluationItemModel>();

            foreach (var item in entries)
            {
                model.Add(new EvaluationItemModel
                {
                    Image = _appHelper.GetImagePath(item.Pictures.Any(s=>s.Priority!=0)? item.Pictures.OrderByDescending(s => s.Priority).First().Url : item.WebsiteAddInfor.Images.OrderByDescending(s=>s.Priority).ThenBy(s=>s.Type).ThenBy(s=>s.Size).First().Url, "app.png"),
                    Name = item.DisplayName,
                    Url = "entries/index/" + item.Id,
                    CommentCount = item.CommentCount,
                    ReadCount = item.ReaderCount,
                    Articles=item.Articles.OrderByDescending(s=>s.Priority).ThenByDescending(s=>s.Type).Take(4).Select(s=>new EvaluationArticleItemModel
                    {
                        Id = s.Id,
                        Image = _appHelper.GetImagePath(s.MainPicture, "app.png"),
                        Name=s.Name,
                        OriginalAuthor = string.IsNullOrWhiteSpace(s.OriginalAuthor) ? s.CreateUser.UserName : s.OriginalAuthor,
                        Type = s.Type,
                    }).ToList()
                });
            }

            return model;
        }


        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<LatastCommentItemModel>>> ListLatastComments()
        {
            var comments = await _commentRepository.GetAll().AsNoTracking()
                .Include(s=>s.ApplicationUser)
                .Where(s => s.IsHidden == false&&string.IsNullOrWhiteSpace(s.Text)==false)
                .OrderByDescending(s => s.CommentTime)
                .Take(6)
                .ToListAsync();

            var model = new List<LatastCommentItemModel>();

            foreach (var item in comments)
            {
                model.Add(new LatastCommentItemModel
                {
                    Url = item.EntryId != null ? $"entries/index/{item.EntryId}" : item.ArticleId != null ? $"articles/index/{item.ArticleId}" : item.Periphery != null ? $"peripheries/index/{item.PeripheryId}" : item.LotteryId != null ? $"lotteries/index/{item.LotteryId}" : item.VoteId != null ? $"votes/index/{item.VoteId}" : item.VideoId != null ? $"videoes/index/{item.VideoId}" : $"space/index/{item.ApplicationUserId}",
                    UserName = item.ApplicationUser.UserName,
                    Time = item.CommentTime.ToTimeFromNowString(),
                    Content = _appHelper.MarkdownToHtml(item.Text),
                    UserImage = _appHelper.GetImagePath(item.ApplicationUser.PhotoPath, "user.png")
                });
            }

            return model;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<HotTagItemModel>>> ListHotTags()
        {
            var entries = await _tagRepository.GetAll().AsNoTracking()
                .Include(s=>s.Entries)
                .Where(s => s.Entries.Count >= 6 )
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false)
                .Where(s=>s.Name.Contains("STAFF")==false&& s.Name.Contains("配音") == false && s.Name.Contains("城市群") == false && s.Name.Contains("声线") == false && s.Name.Contains("字幕") == false)
                .OrderByDescending(s => s.Priority).ThenByDescending(s=>s.Entries.Count)
                .Take(15)
                .ToListAsync();

            var model = new List<HotTagItemModel>();

            foreach (var item in entries)
            {
                model.Add(new HotTagItemModel
                {
                    Name = item.Name,
                    Url = "tags/index/" + item.Id,
                });
            }

            return model;
        }

        #region 存档
        /// <summary>
        /// 获取即将发售游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeNewestGameViewAsync()
        {
            return await _homeService.GetHomeNewestGameViewAsync();
        }
        /// <summary>
        /// 获取近期编辑的游戏或制作组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeRecentEditViewAsync()
        {
            return await _homeService.GetHomeRecentEditViewAsync();

        }
        /// <summary>
        /// 获取近期发售的游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeRecentIssuelGameViewAsync()
        {
            return await _homeService.GetHomeRecentIssuelGameViewAsync();

        }

        /// <summary>
        /// 获取友情链接 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeFriendLinksViewAsync()
        {
            return await _homeService.GetHomeFriendLinksViewAsync();

        }

        /// <summary>
        /// 获取通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeNoticesViewAsync()
        {
            return await _homeService.GetHomeNoticesViewAsync();

        }
        /// <summary>
        /// 获取最近发布的文章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeArticlesViewAsync()
        {
            return await _homeService.GetHomeArticlesViewAsync();

        }
        /// <summary>
        /// 获取最近发布的视频
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<HomeItemModel>>> GetHomeVideosViewAsync()
        {
            return await _homeService.GetHomeVideosViewAsync();

        }
        #endregion
    }
}
