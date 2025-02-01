using CnGalWebSite.APIServer.Application.BackUpArchives;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/backuparchives/[action]")]

    public class BackUpArchiveAPIController : ControllerBase
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IQueryService _queryService;
        private readonly IBackUpArchiveService _backUpArchiveService;

        public BackUpArchiveAPIController(IBackUpArchiveService backUpArchiveService, IRepository<BackUpArchive, long> backUpArchiveRepository,
          IRepository<Entry, int> entryRepository, IQueryService queryService)
        {
            _entryRepository = entryRepository;
            _backUpArchiveRepository = backUpArchiveRepository;
            _backUpArchiveService = backUpArchiveService;
            _queryService = queryService;
        }

        [HttpPost]
        public async Task<QueryResultModel<BackUpArchiveOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<BackUpArchive, long>(_backUpArchiveRepository.GetAll().AsSingleQuery().Include(s=>s.Entry).Include(s=>s.Article), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || ((s.Article!=null&&s.Article.Name.Contains(model.SearchText)) || (s.Entry != null && s.Entry.Name.Contains(model.SearchText))));

            return new QueryResultModel<BackUpArchiveOverviewModel>
            {
                Items = await items.Select(s => new BackUpArchiveOverviewModel
                {
                    Id = s.Id,
                    LastBackUpTime = s.LastBackUpTime,
                    IsLastFail = s.IsLastFail,
                    LastTimeUsed = s.LastTimeUsed,
                    Type=s.Entry!=null? BackUpArchiveType.Entry:  BackUpArchiveType.Article,
                    ObjectId= s.Entry != null ? s.Entry.Id : s.Article != null ? s.Article.Id : 0,
                    ObjectName = s.Entry != null ? s.Entry.Name : s.Article != null ? s.Article.Name : null,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<ActionResult<Result>> RunBackUpArchiveAsync(RunBackUpArchiveModel model)
        {
            var backUpArchives = await _backUpArchiveRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ToListAsync();

            if (backUpArchives.Count == 0)
            {
                return new Result { Successful = false, Error = "无法找到Id：" + model.Ids.First().ToString() + " 的备份记录" };
            }
            foreach (var item in backUpArchives)
            {
                if (item.EntryId != null && item.EntryId > 0)
                {
                    await _backUpArchiveService.BackUpEntry(item, await _entryRepository.GetAll().Where(s => s.Id == item.EntryId).Select(s => s.Name).FirstOrDefaultAsync());
                }
                else if (item.ArticleId != null && item.ArticleId > 0)
                {
                    await _backUpArchiveService.BackUpArticle(item);
                }
                else
                {
                    return new Result { Successful = false, Error = "关联词条或文章Id必须为有效值" };
                }
            }

            return new Result { Successful = true };
        }
    }
}
