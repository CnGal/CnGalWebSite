
using CnGalWebSite.APIServer.Application.ElasticSearches;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Home;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
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
        public async Task<ActionResult<List<Carousel>>> GetHomeCarouselsViewAsync()
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
                Carousels = await _homeService.GetHomeCarouselsViewAsync()
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

        /// <summary>
        /// 获取编辑记录概览
        /// </summary>
        /// <param name="id">要对比的编辑记录</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ExaminesOverviewViewModel>> GetEditRecordsOverview(long id)
        {
            var examine = await _examineRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == id && s.IsPassed == true);
            if (examine == null)
            {
                return NotFound("无法找到该审核");
            }

            var model = new ExaminesOverviewViewModel();
            if (examine.EntryId != null)
            {
                var entry = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.EntryId);
                if (entry == null)
                {
                    return NotFound("无法找到审核记录对应的词条");
                }

                model.ObjectId = entry.Id;
                model.ObjectName = entry.DisplayName;
                model.ObjectBriefIntroduction = entry.BriefIntroduction;
                model.Image = (entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? _appHelper.GetImagePath(entry.MainPicture, "app.png") : _appHelper.GetImagePath(entry.Thumbnail, "user.png");
                model.IsThumbnail = (entry.Type == EntryType.Game || entry.Type == EntryType.ProductionGroup) ? false : true;
                model.Type = ExaminedNormalListModelType.Entry;

                var examines = await _examineRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).Include(s => s.Entry)
                    .Where(s => s.EntryId == entry.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();

                var examinedView = new Models.ExaminedViewModel();

                foreach (var item in examines)
                {
                    if (await _examineService.GetExamineView(examinedView, item))
                    {

                        model.Examines.Add(new EditRecordAloneViewModel
                        {
                            ApplyTime = item.ApplyTime,
                            Comments = item.Comments,
                            EditOverview = examinedView.EditOverview,
                            Id = item.Id,
                            Note = item.Note,
                            Operation = item.Operation,
                            PassedAdminName = item.PassedAdminName,
                            PassedTime = item.PassedTime.Value,
                            UserId = item.ApplicationUserId,
                            IsSelected = item.Id == id,
                            UserName = item.ApplicationUser.UserName
                        });
                    }

                }

                return model;
            }
            else if(examine.ArticleId!=null)
            {
                var article = await _articleRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == examine.ArticleId);
                if (article == null)
                {
                    return NotFound("无法找到审核记录对应的文章");
                }

                model.ObjectId = article.Id;
                model.ObjectName = article.DisplayName;
                model.ObjectBriefIntroduction = article.BriefIntroduction;
                model.Image = _appHelper.GetImagePath(article.MainPicture, "app.png");
                model.IsThumbnail = false;
                model.Type = ExaminedNormalListModelType.Article;

                var examines = await _examineRepository.GetAll().AsNoTracking().Include(s => s.ApplicationUser).Include(s => s.Entry)
                    .Where(s => s.ArticleId == article.Id && s.IsPassed == true).OrderBy(s => s.Id).ToListAsync();

                var examinedView = new Models.ExaminedViewModel();

                foreach (var item in examines)
                {
                    if (await _examineService.GetExamineView(examinedView, item))
                    {
                        model.Examines.Add(new EditRecordAloneViewModel
                        {
                            ApplyTime = item.ApplyTime,
                            Comments = item.Comments,
                            EditOverview = examinedView.EditOverview,
                            Id = item.Id,
                            Note = item.Note,
                            Operation = item.Operation,
                            PassedAdminName = item.PassedAdminName,
                            PassedTime = item.PassedTime.Value,
                            UserId = item.ApplicationUserId,
                            IsSelected = item.Id == id,
                            UserName = item.ApplicationUser.UserName
                        });
                    }

                }

                return model;
            }
            else
            {
                return BadRequest("无效的类型");
            }
        }
    }
}
