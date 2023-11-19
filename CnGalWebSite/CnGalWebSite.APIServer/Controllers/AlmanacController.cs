using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Almanacs;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AlmanacController : ControllerBase
    {
        public readonly IRepository<Almanac, long> _almanacRepository;
        public readonly IRepository<AlmanacArticle, long> _almanacArticleRepository;
        public readonly IRepository<AlmanacEntry, long> _almanacEntryRepository;
        public readonly IRepository<Entry, int> _entryRepository;
        public readonly IRepository<Article, long> _articleRepository;
        public readonly IAppHelper _appHelper;
        private readonly IQueryService _queryService;
        private readonly IArticleService _articleService;
        private readonly IEntryService _entryService;

        public AlmanacController(IRepository<Almanac, long> almanacRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository, IQueryService queryService, IRepository<AlmanacArticle, long> almanacArticleRepository,
            IRepository<AlmanacEntry, long> almanacEntryRepository, IArticleService articleService, IEntryService entryService)
        {
            _almanacRepository = almanacRepository;
            _appHelper = appHelper;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _queryService = queryService;
            _almanacArticleRepository = almanacArticleRepository;
            _almanacEntryRepository = almanacEntryRepository;
            _articleService = articleService;
            _entryService = entryService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<AlmanacViewModel>> GetAsync([FromQuery] long id)
        {
            var item = await _almanacRepository.GetAll().AsNoTracking()
                .Include(s => s.Articles).ThenInclude(s => s.Article)
                .Include(s => s.Entries).ThenInclude(s => s.Entry)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.WebsiteAddInfor).ThenInclude(s => s.Images)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation).ThenInclude(s => s.Information)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.Information)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.Tags)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.Pictures)
                .Include(s => s.Entries).ThenInclude(s => s.Entry).ThenInclude(s => s.Releases)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new AlmanacViewModel
            {
                BriefIntroduction = item.BriefIntroduction,
                CreateTime = item.CreateTime,
                UpdateTime = item.UpdateTime,
                Id = item.Id,
                Name = item.Name,
                Year = item.Year,
                Articles = item.Articles.Where(s => s.Hide == false).OrderByDescending(s => s.Priority).ThenBy(s => s.Article.PubishTime).Select(s => new AlmanacArticleViewModel
                {
                    Article = _articleService.GetArticleViewModelAsync(s.Article),
                    Hide = s.Hide,
                    Id = s.Id,
                    Image = s.Image,
                    Priority = s.Priority,
                }).ToList(),
            };

            foreach (var info in item.Entries.Where(s => s.Hide == false).OrderByDescending(s => s.Priority).ThenBy(s => s.Entry.PubulishTime))
            {
                var temp = new AlmanacEntryViewModel
                {
                    Hide = info.Hide,
                    Id = info.Id,
                    Image = info.Image,
                    Priority = info.Priority,
                    PublishTime=info.Entry.PubulishTime
                };
                temp.Entry = await _entryService.GetEntryIndexViewModelAsync(info.Entry);
                model.Entries.Add(temp);
            }

            return model;
        }


        [HttpGet]
        public async Task<ActionResult<AlmanacEditModel>> EditAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new AlmanacEditModel();
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var item = await _almanacRepository.GetAll()
                .Include(s => s.Entries).ThenInclude(s => s.Entry)
                .Include(s => s.Articles).ThenInclude(s => s.Article)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new AlmanacEditModel
            {
                BriefIntroduction = item.BriefIntroduction,
                Id = id,
                Name = item.Name,
                Year = item.Year,
                Articles = item.Articles.Select(s => new AlmanacArticleEditModel
                {
                    ArticleId = s.ArticleId,
                    ArticleName = s.Article.Name,
                    Id = s.Id,
                    Image = s.Image,
                    Hide = s.Hide,
                    Priority = s.Priority,
                }).ToList(),
                Entries = item.Entries.Select(s => new AlmanacEntryEditModel
                {
                    EntryId = s.EntryId,
                    EntryName = s.Entry.Name,
                    Id = s.Id,
                    Image = s.Image,
                    Hide = s.Hide,
                    Priority = s.Priority,
                }).ToList()
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAsync(AlmanacEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Successful)
            {
                return vail;
            }
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            Almanac item = null;
            if (model.Id == 0)
            {
                item = await _almanacRepository.InsertAsync(new Almanac
                {
                    BriefIntroduction = model.BriefIntroduction,
                    Year = model.Year,
                    Name = model.Name,
                    CreateTime = DateTime.Now.ToCstTime(),
                });
                model.Id = item.Id;
                _almanacRepository.Clear();
            }

            item = await _almanacRepository.GetAll()
                .Include(s => s.Entries).ThenInclude(s => s.Entry)
                .Include(s => s.Articles).ThenInclude(s => s.Article)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (item == null)
            {
                return new Result { Successful = false, Error = "项目不存在" };
            }

            item.BriefIntroduction = model.BriefIntroduction;
            item.Name = model.Name;
            item.Year = model.Year;

            //转换词条列表
            var entries = new List<AlmanacEntry>();
            foreach (var info in model.Entries)
            {
                var id = (await _entryRepository.FirstOrDefaultAsync(s => s.Name == info.EntryName))?.Id;
                if (id != null)
                {
                    entries.Add(new AlmanacEntry
                    {
                        EntryId = id.Value,
                        Id = info.Id,
                        Image = info.Image,
                        Hide = info.Hide,
                        Priority = info.Priority,
                    });
                }
            }
            //转换文章列表
            var articles = new List<AlmanacArticle>();
            foreach (var info in model.Articles)
            {
                var id = (await _articleRepository.FirstOrDefaultAsync(s => s.Name == info.ArticleName))?.Id;
                if (id != null)
                {
                    articles.Add(new AlmanacArticle
                    {
                        ArticleId = id.Value,
                        Id = info.Id,
                        Image = info.Image,
                        Hide = info.Hide,
                        Priority = info.Priority,
                    });
                }
            }

            //词条
            item.Entries.RemoveAll(s => entries.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Entries)
            {
                var temp = entries.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.Image = temp.Image;
                    info.Hide = temp.Hide;
                    info.Priority = temp.Priority;
                    info.EntryId = temp.EntryId;
                    info.AlmanacId = item.Id;
                }
            }
            item.Entries.AddRange(entries.Where(s => s.Id == 0).Select(s => new AlmanacEntry
            {
                EntryId = s.EntryId,
                Image = s.Image,
                Hide = s.Hide,
                Priority = s.Priority,
                AlmanacId = item.Id,
            }));

            //文章
            item.Articles.RemoveAll(s => articles.Select(s => s.Id).Contains(s.Id) == false);
            foreach (var info in item.Articles)
            {
                var temp = articles.FirstOrDefault(s => s.Id == info.Id);
                if (temp != null)
                {
                    info.Image = temp.Image;
                    info.Hide = temp.Hide;
                    info.Priority = temp.Priority;
                    info.ArticleId = temp.ArticleId;
                    info.AlmanacId = item.Id;
                }
            }
            item.Articles.AddRange(articles.Where(s => s.Id == 0).Select(s => new AlmanacArticle
            {
                ArticleId = s.ArticleId,
                Image = s.Image,
                Hide = s.Hide,
                Priority = s.Priority,
                AlmanacId = item.Id
            }));

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _almanacRepository.UpdateAsync(item);

            return new Result { Successful = true, Error = model.Id.ToString() };
        }

        [HttpPost]
        public async Task<QueryResultModel<AlmanacOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Almanac, long>(_almanacRepository.GetAll().AsSingleQuery().Include(s => s.Articles).Include(s => s.Entries), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.BriefIntroduction.Contains(model.SearchText)));

            return new QueryResultModel<AlmanacOverviewModel>
            {
                Items = await items.Select(s => new AlmanacOverviewModel
                {
                    Id = s.Id,
                    UpdateTime = s.UpdateTime,
                    Name = s.Name,
                    ArticleCount = s.Articles.Count,
                    BriefIntroduction = s.BriefIntroduction,
                    EntryCount = s.Entries.Count,
                    Year = s.Year,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
