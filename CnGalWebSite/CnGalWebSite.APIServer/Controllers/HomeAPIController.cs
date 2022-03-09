
using CnGalWebSite.APIServer.Application.ElasticSearches;
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
        private readonly ISearchService _searchService;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IHomeService _homeService;
        private readonly IElasticsearchService _elasticsearchService;
        private readonly IExamineService _examineService;
        private readonly IAppHelper _appHelper;

        public HomeAPIController(ISearchService searchService, IElasticsearchService elasticsearchService, IAppHelper appHelper, IRepository<Article, long> articleRepository,
        IRepository<Entry, int> entryRepository, IHomeService homeService, IExamineService examineService, IRepository<Examine, long> examineRepository)
        {
            _searchService = searchService;
            _entryRepository = entryRepository;
            _homeService = homeService;
            _elasticsearchService = elasticsearchService;
            _examineService = examineService;
            _appHelper = appHelper;
            _examineRepository = examineRepository;
            _articleRepository = articleRepository;
        }

        /// <summary>
        /// 获取即将发售游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeNewestGameViewAsync()
        {
            return await _homeService.GetHomeNewestGameViewAsync();
        }
        /// <summary>
        /// 获取近期编辑的游戏或制作组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeRecentEditViewAsync()
        {
            return await _homeService.GetHomeRecentEditViewAsync();

        }
        /// <summary>
        /// 获取近期发售的游戏
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeRecentIssuelGameViewAsync()
        {
            return await _homeService.GetHomeRecentIssuelGameViewAsync();

        }
        /// <summary>
        /// 获取 优先级最高的游戏或制作组
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeFriendEntriesViewAsync()
        {
            return await _homeService.GetHomeFriendEntriesViewAsync();

        }

        /// <summary>
        /// 获取友情链接 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeFriendLinksViewAsync()
        {
            return await _homeService.GetHomeFriendLinksViewAsync();

        }

        /// <summary>
        /// 获取通知
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeNoticesViewAsync()
        {
            return await _homeService.GetHomeNoticesViewAsync();

        }
        /// <summary>
        /// 获取最近发布的文章
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<EntryHomeAloneViewModel>>> GetHomeArticlesViewAsync()
        {
            return await _homeService.GetHomeArticlesViewAsync();

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
            return await _homeService.GetHomeCarouselsViewAsync();

        }
        /// <summary>
        /// 获取合并后的主页汇总数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<HomeViewModel>> GetHomeViewAsync()
        {
            var model = new HomeViewModel
            {
                NewestGame = await _homeService.GetHomeNewestGameViewAsync(),
                RecentIssuelGame = await _homeService.GetHomeRecentIssuelGameViewAsync(),
                RecentEditEntries = await _homeService.GetHomeRecentEditViewAsync(),
                FriendLinks = await _homeService.GetHomeFriendLinksViewAsync(),
                Notices = await _homeService.GetHomeNoticesViewAsync(),
                Articles = await _homeService.GetHomeArticlesViewAsync(),
                //Carousels = await _homeService.GetHomeCarouselsViewAsync()
            };

            return model;
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<SearchViewModel>> SearchAsync(GetSearchInput input)
        {
            try
            {
                var model = new SearchViewModel();
                var dtos = input.StartIndex == -1 ?
                    await _elasticsearchService.QueryAsync(input.CurrentPage, input.MaxResultCount, input.FilterText, input.ScreeningConditions, input.Sorting, QueryType.Page) :
                    await _elasticsearchService.QueryAsync(input.StartIndex, input.MaxResultCount, input.FilterText, input.ScreeningConditions, input.Sorting, QueryType.Index);
                dtos.Data = dtos.Data.ToList();

                model.pagedResultDto = dtos;

                return model;
            }
            catch (Exception ex)
            {
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
                                Id = 636,
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
                .Where(s => s.Type == ArticleType.Notice && s.Name.Contains("更新"))
                .Select(s => new DocumentViewModel { Id = s.Id, Title = s.DisplayName.Replace("网站","") })
                .ToListAsync();
            model.Add(new DocumentViewModel
            {
                Icon = "mdi-cloud-upload-outline ",
                Id = 99998,
                Title = "更新日志",
                Children = articles.OrderByDescending(s => s.Id).ToList()
            });


            //公告
            articles = await _articleRepository.GetAll().AsNoTracking()
              .Where(s => s.Type == ArticleType.Notice && s.Name.Contains("更新") == false && s.Id != 150 && s.Id != 225 && s.Id != 636)
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
             .Select(s => new DocumentViewModel { Id = s.Id, Title = s.Name.Replace("CnGal每周速报（","").Replace("）","") })
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


    }
}
