using CnGalWebSite.APIServer.Application.Charts;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/perfections/[action]")]
    public class PerfectionAPIController : ControllerBase
    {
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<PerfectionOverview, long> _perfectionOverviewRepository;
        private readonly IRepository<Perfection, long> _perfectionRepository;
        private readonly IAppHelper _appHelper;
        private readonly IPerfectionService _perfectionService;
        private readonly IExamineService _examineService;
        private readonly IChartService _chartService;
        private readonly IConfiguration _configuration;
        private readonly IQueryService _queryService;
        private readonly IRepository<PerfectionCheck, long> _perfectionCheckRepository;

        public PerfectionAPIController(IRepository<Examine, long> examineRepository, IConfiguration configuration,
        IAppHelper appHelper, IRepository<Perfection, long> perfectionRepository, IQueryService queryService, IRepository<PerfectionCheck, long> perfectionCheckRepository,
        IPerfectionService perfectionService, IRepository<PerfectionOverview, long> perfectionOverviewRepository,
         IExamineService examineService, IChartService chartService)
        {
            _appHelper = appHelper;
            _examineRepository = examineRepository;
            _perfectionService = perfectionService;
            _perfectionOverviewRepository = perfectionOverviewRepository;
            _examineService = examineService;
            _chartService = chartService;
            _configuration = configuration;
            _perfectionRepository = perfectionRepository;
            _queryService = queryService;
            _perfectionCheckRepository = perfectionCheckRepository;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PerfectionInforTipViewModel>> GetEntryPerfectionAsync(int id)
        {
            return await _perfectionService.GetEntryPerfection(id);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<PerfectionCheckViewModel>>> GetEntryPerfectionCheckListAsync(long id)
        {
            return await _perfectionService.GetEntryPerfectionCheckList(id);
        }
        [AllowAnonymous]
        [HttpGet("{level}")]
        public async Task<ActionResult<List<PerfectionInforTipViewModel>>> GetPerfectionLevelRadomListAsync(PerfectionLevel level)
        {
            return await _perfectionService.GetPerfectionLevelRadomListAsync(level);
        }
        [AllowAnonymous]
        [HttpGet("{level}")]
        public async Task<ActionResult<List<PerfectionCheckViewModel>>> GetPerfectionCheckLevelRadomListAsync(PerfectionCheckLevel level)
        {
            return await _perfectionService.GetPerfectionCheckLevelRadomListAsync(level);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PerfectionLevelOverviewModel>> GetPerfectionLevelOverviewAsync()
        {
            return await _perfectionService.GetPerfectionLevelOverview();
        }

        private const int MaxCountLineDay = 60;

        /// <summary>
        /// 获取 编辑概览、完善度 图表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<LineChartModel>> GetPerfectionLineChartAsync([FromQuery] LineChartType type, [FromQuery] long afterTime, [FromQuery] long beforeTime)
        {
            if (type == LineChartType.Edit || type == LineChartType.PerfectionLevel || type == LineChartType.StatisticalData)
            {
                var after = afterTime.ToString().TransTime();
                var before = beforeTime.ToString().TransTime();

                return await _chartService.GetLineChartAsync(type, after, before);
            }
            else
            {
                return NotFound("其他类型图表请使用更高权限的接口");

            }

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ExaminedNormalListModel>>> GetRecentlyEditListAsync()
        {
            return await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => (s.PrepositionExamineId == null || s.PrepositionExamineId == -1) && s.IsPassed == true
            &&s.ApplicationUserId!= _configuration["ExamineAdminId"]&& s.ApplicationUserId != _configuration["NewsAdminId"]
            && s.Operation != Operation.UserMainPage && s.Operation != Operation.EditUserMain && s.Operation != Operation.PubulishComment && s.Operation != Operation.EditPlayedGameMain && s.Operation != Operation.RequestUserCertification && s.Operation != Operation.EditFavoriteFolderMain).OrderByDescending(s => s.Id).Take(12), true);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<QueryResultModel<PerfectionOverviewModel>> ListPerfections(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Perfection, long>(_perfectionRepository.GetAll().AsSingleQuery().Include(s => s.Entry)
                .Where(s => s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false && s.Entry.Type == EntryType.Game), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Entry.Name.Contains(model.SearchText)));

            return new QueryResultModel<PerfectionOverviewModel>
            {
                Items = await items.Select(s => new PerfectionOverviewModel
                {
                    Id = s.EntryId,
                    Name = s.Entry.Name,
                    Grade = s.Grade,
                    VictoryPercentage = s.VictoryPercentage,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<QueryResultModel<PerfectionCheckOverviewModel>> ListPerfectionChecks(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<PerfectionCheck, long>(_perfectionCheckRepository.GetAll().AsSingleQuery().Include(s => s.Perfection).ThenInclude(s => s.Entry)
                .Where(s => s.Perfection.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Perfection.Entry.Name) == false && s.Perfection.Entry.Type == EntryType.Game), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Perfection.Entry.Name.Contains(model.SearchText)));

            return new QueryResultModel<PerfectionCheckOverviewModel>
            {
                Items = await items.Select(s => new PerfectionCheckOverviewModel
                {
                    Id = s.Perfection.EntryId,
                    Name = s.Perfection.Entry.Name,
                    Type = s.CheckType,
                    Grade = s.Grade,
                    VictoryPercentage = s.VictoryPercentage,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

    }
}
