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
        public async Task<ActionResult<LineChartModel>> GetEditCountLineAsync([FromQuery] LineChartType type, [FromQuery] long afterTime, [FromQuery] long beforeTime)
        {
            if(type!= LineChartType.Edit)
            {
                return NotFound("其他类型图表请使用更高权限的接口");
            }
            var after = DateTime.FromBinary(afterTime);
            var before = DateTime.FromBinary(beforeTime);

            return await _chartService.GetLineChartAsync(type, after, before);
        }

        /// <summary>
        /// 获取 统计数据 图表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetStatisticalDataLineAsync()
        {
            try
            {
                var tempDateTimeNow = DateTime.Now.ToCstTime();

                //获取数据
                var datas = await _perfectionOverviewRepository.GetAll().Where(s => s.LastUpdateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
                   .Select(n => new { Time = n.LastUpdateTime.Date, n.AverageValue, n.Median, n.Mode, n.StandardDeviation })
                   .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
                   .ToListAsync();

                var averageValues = datas.Select(s => new CountLineModel { Count = s.AverageValue, Time = s.Time }).ToList();
                var medians = datas.Select(s => new CountLineModel { Count = s.Median, Time = s.Time }).ToList();
                var modes = datas.Select(s => new CountLineModel { Count = s.Mode, Time = s.Time }).ToList();
                var standardDeviations = datas.Select(s => new CountLineModel { Count = s.StandardDeviation, Time = s.Time }).ToList();

                var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["平均值"] = averageValues, ["中位数"] = medians, ["众数"] = modes, ["标准差"] = standardDeviations }, "日期", "数目", "统计数据概览");
                return temp;
            }
            catch (Exception)
            {
                return NotFound();
            }

        }

        /// <summary>
        /// 获取 全站完善度 图表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<BootstrapBlazor.Components.ChartDataSource>> GetPerfectionLevelCountLineAsync()
        {

            var tempDateTimeNow = DateTime.Now.ToCstTime();

            //获取数据
            var datas = await _perfectionOverviewRepository.GetAll().Where(s => s.LastUpdateTime.Date > tempDateTimeNow.Date.AddDays(-MaxCountLineDay))
               // 先进行了时间字段变更为String字段，切只保留到天
               // 采用拼接的方式
               .Select(n => new { Time = n.LastUpdateTime.Date, n.ToBeImprovedCount, n.GoodCount, n.ExcellentCount })
               .Sort("Time", BootstrapBlazor.Components.SortOrder.Asc)
               .ToListAsync();

            var toBeImprovedCounts = datas.Select(s => new CountLineModel { Count = s.ToBeImprovedCount, Time = s.Time }).ToList();
            var goodCounts = datas.Select(s => new CountLineModel { Count = s.GoodCount, Time = s.Time }).ToList();
            var excellentCounts = datas.Select(s => new CountLineModel { Count = s.ExcellentCount, Time = s.Time }).ToList();

            var temp = _appHelper.GetCountLine(new Dictionary<string, List<CountLineModel>> { ["已完善"] = excellentCounts, ["待完善"] = goodCounts, ["急需完善"] = toBeImprovedCounts }, "日期", "数目", "全站完善度概览");
            return temp;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<ExaminedNormalListModel>>> GetRecentlyEditListAsync()
        {
            return await _examineService.GetExaminesToNormalListAsync(_examineRepository.GetAll().Where(s => (s.PrepositionExamineId == null || s.PrepositionExamineId == -1) && s.IsPassed == true
            && s.Operation != Operation.UserMainPage && s.Operation != Operation.EditUserMain && s.Operation != Operation.PubulishComment).OrderByDescending(s => s.Id).Take(6), true);
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
