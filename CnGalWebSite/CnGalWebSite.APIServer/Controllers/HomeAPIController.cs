using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Home;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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
        private readonly IHomeService _homeService;
        private readonly IExamineService _examineService;
        private readonly IAppHelper _appHelper;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<HomeAPIController> _logger;

        public HomeAPIController(ISearchHelper searchHelper, IAppHelper appHelper, IRepository<Article, long> articleRepository, IHostApplicationLifetime applicationLifetime, ILogger<HomeAPIController> logger,
        IRepository<Entry, int> entryRepository, IHomeService homeService, IExamineService examineService, IRepository<Examine, long> examineRepository)
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
        public async Task<List<PersonalRecommendModel>> GetPersonalizedRecommendations(List<PersonalRecommendModel> model)
        {
            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false && s.Type == EntryType.Game && model.Select(s => s.ObjectId).Contains(s.Id) == false)
                .Select(s => s.Id)
                .ToListAsync();

            return entryIds.Random().Take(5).Select(s => new PersonalRecommendModel
            {
                DisplayType = PersonalRecommendDisplayType.PlainText,
                ObjectId = s
            }).ToList();
        }


    }
}
