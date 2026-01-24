using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.Tables;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/tables/[action]")]
    public class TablesAPIController : ControllerBase
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<BasicInforTableModel, long> _basicInforTableModelRepository;
        private readonly IRepository<GroupInforTableModel, long> _groupInforTableModelRepository;
        private readonly IRepository<StaffInforTableModel, long> _staffInforTableModelRepository;
        private readonly IRepository<RoleInforTableModel, long> _roleInforTableModelRepository;
        private readonly IRepository<StoreInfo, long> _storeInfoRepository;
        private readonly IRepository<MakerInforTableModel, long> _makerInforTableModelRepository;
        private readonly IRepository<GameScoreTableModel, long> _gameScoreTableRepository;
        private readonly ITableService _tableService;
        private readonly IQueryService _queryService;


        public TablesAPIController(IRepository<BasicInforTableModel, long> basicInforTableModelRepository, IRepository<GameScoreTableModel, long> gameScoreTableRepository,
        IRepository<Article, long> articleRepository, IRepository<Entry, int> entryRepository, IRepository<Examine, long> examineRepository, IRepository<GroupInforTableModel, long> groupInforTableModelRepository,
            IRepository<StaffInforTableModel, long> staffInforTableModelRepository, IRepository<RoleInforTableModel, long> roleInforTableModelRepository, IRepository<StoreInfo, long> storeInfoRepository,
           IRepository<MakerInforTableModel, long> makerInforTableModelRepository, ITableService tableService, IQueryService queryService)
        {
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _articleRepository = articleRepository;
            _basicInforTableModelRepository = basicInforTableModelRepository;
            _groupInforTableModelRepository = groupInforTableModelRepository;
            _staffInforTableModelRepository = staffInforTableModelRepository;
            _roleInforTableModelRepository = roleInforTableModelRepository;
            _storeInfoRepository = storeInfoRepository;
            _makerInforTableModelRepository = makerInforTableModelRepository;
            _gameScoreTableRepository = gameScoreTableRepository;
            _tableService = tableService;
            _queryService = queryService;
        }

        [HttpPost]
        public async Task<QueryResultModel<BasicInforTableModel>> ListGames(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<BasicInforTableModel, long>(_basicInforTableModelRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<BasicInforTableModel>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


        [HttpPost]
        public async Task<QueryResultModel<GroupInforTableModel>> ListGroups(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<GroupInforTableModel, long>(_groupInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<GroupInforTableModel>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<StaffInforTableModel>> ListStaffs(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<StaffInforTableModel, long>(_staffInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.GameName.Contains(model.SearchText)));

            return new QueryResultModel<StaffInforTableModel>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<MakerInforTableModel>> ListMakers(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<MakerInforTableModel, long>(_makerInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<MakerInforTableModel>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


        [HttpPost]
        public async Task<QueryResultModel<RoleInforTableModel>> ListRoles(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<RoleInforTableModel, long>(_roleInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<RoleInforTableModel>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


        [HttpGet]
        public async Task<ActionResult<TableViewModel>> GetTableViewAsync()
        {
            var model = new TableViewModel
            {
                EntriesCount = await _entryRepository.CountAsync(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false),
                ArticlesCount = await _articleRepository.LongCountAsync(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
            };
            if (await _examineRepository.GetAll().AnyAsync())
            {
                model.LastEditTime = await _examineRepository.GetAll().MaxAsync(s => s.ApplyTime);
            }

            return model;
        }

        [HttpPost]
        public async Task<QueryResultModel<StoreInfoViewModel>> ListStoreInfo(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<StoreInfo, long>(_storeInfoRepository.GetAll().AsSingleQuery().Where(s => s.State != StoreState.None), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)));

            return new QueryResultModel<StoreInfoViewModel>
            {
                Items = await items.Select(s => new StoreInfoViewModel
                {
                    State = s.State,
                    CurrencyCode = s.CurrencyCode,
                    CutLowest = s.CutLowest,
                    CutNow = s.CutNow,
                    EstimationOwnersMax = s.EstimationOwnersMax,
                    EstimationOwnersMin = s.EstimationOwnersMin,
                    EvaluationCount = s.EvaluationCount,
                    Link = s.Link,
                    OriginalPrice = s.OriginalPrice,
                    PlatformName = s.PlatformName,
                    PlatformType = s.PlatformType,
                    PlayTime = s.PlayTime,
                    PriceLowest = s.PriceLowest,
                    PriceNow = s.PriceNow,
                    RecommendationRate = s.RecommendationRate,
                    UpdateTime = s.UpdateTime,
                    UpdateType = s.UpdateType,
                    Name = s.Name,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<GameScoreTableModel>> ListGameScores(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<GameScoreTableModel, long>(_gameScoreTableRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.GameName.Contains(model.SearchText)));


            var ret = await items.ToListAsync();

            return new QueryResultModel<GameScoreTableModel>
            {
                Items = ret,
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<ActionResult<EChartsTreeMapOptionModel>> GetGroupGameRoleTreeMapAsync()
        {
            return await _tableService.GetGroupGameRoleTreeMap();
        }

        [HttpGet]
        public async Task<ActionResult<EChartsGraphOptionModel>> GetEntryGraph()
        {
            return await _tableService.GetEntryGraph();
        }
    }
}
