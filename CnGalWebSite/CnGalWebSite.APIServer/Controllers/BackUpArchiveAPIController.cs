using CnGalWebSite.APIServer.Application.BackUpArchives;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/backuparchives/[action]")]

    public class BackUpArchiveAPIController : ControllerBase
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;

        private readonly IBackUpArchiveService _backUpArchiveService;

        public BackUpArchiveAPIController(IBackUpArchiveService backUpArchiveService, IRepository<BackUpArchive, long> backUpArchiveRepository,
          IRepository<Entry, int> entryRepository)
        {
            _entryRepository = entryRepository;
            _backUpArchiveRepository = backUpArchiveRepository;
            _backUpArchiveService = backUpArchiveService;
        }

        [HttpGet]
        public async Task<ActionResult<ListBackUpArchivesInforViewModel>> ListBackUpArchivesAsync()
        {
            var model = new ListBackUpArchivesInforViewModel
            {
                All = await _backUpArchiveRepository.CountAsync(),
                IsLastFail = await _backUpArchiveRepository.CountAsync(s => s.IsLastFail == true),
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListBackUpArchiveAloneModel>>> GetBackUpArchiveListAsync(BackUpArchivesPagesInfor input)
        {
            var dtos = await _backUpArchiveService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
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
