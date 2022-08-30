using CnGalWebSite.APIServer.Application.Charts;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Perfections;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/perfections/[action]")]
    public class PerfectionAPIController : ControllerBase
    {
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<PerfectionOverview, long> _perfectionOverviewRepository;
        private readonly IAppHelper _appHelper;
        private readonly IPerfectionService _perfectionService;
        private readonly IExamineService _examineService;
        private readonly IChartService _chartService;

        public PerfectionAPIController(IRepository<Examine, long> examineRepository,
        IAppHelper appHelper,
       IPerfectionService perfectionService, IRepository<PerfectionOverview, long> perfectionOverviewRepository,
         IExamineService examineService, IChartService chartService)
        {
            _appHelper = appHelper;
            _examineRepository = examineRepository;
            _perfectionService = perfectionService;
            _perfectionOverviewRepository = perfectionOverviewRepository;
            _examineService = examineService;
            _chartService = chartService;
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
        /// 获取编辑概览图表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<LineChartModel>> GetPerfectionLineChartAsync([FromQuery] LineChartType type, [FromQuery] long afterTime, [FromQuery] long beforeTime)
        {
            if (type == LineChartType.Edit || type == LineChartType.PerfectionLevel || type == LineChartType.StatisticalData)
            {
                var after = DateTime.FromBinary(afterTime);
                var before = DateTime.FromBinary(beforeTime);

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
            && s.Operation != Operation.UserMainPage && s.Operation != Operation.EditUserMain && s.Operation != Operation.PubulishComment && s.Operation != Operation.RequestUserCertification).OrderByDescending(s => s.Id).Take(12), true);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListPerfectionAloneModel>>> GetPerfectionListAsync(PerfectionsPagesInfor input)
        {
            var dtos = await _perfectionService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListPerfectionCheckAloneModel>>> GetPerfectionCheckListAsync(PerfectionChecksPagesInfor input)
        {
            var dtos = await _perfectionService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }
    }
}
