using CnGalWebSite.DataModel.Sorting;
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
        private static readonly SortMap<BasicInforTableModel, BasicInforSortField> ListGamesSorting =
            new SortMap<BasicInforTableModel, BasicInforSortField>(BasicInforSortField.Id, BasicInforSortField.Id)
            .Add(BasicInforSortField.Id, s => s.Id)
            .Add(BasicInforSortField.RealId, s => s.RealId)
            .Add(BasicInforSortField.Name, s => s.Name)
            .Add(BasicInforSortField.IssueTime, s => s.IssueTime)
            .Add(BasicInforSortField.Original, s => s.Original)
            .Add(BasicInforSortField.ProductionGroup, s => s.ProductionGroup)
            .Add(BasicInforSortField.GamePlatforms, s => s.GamePlatforms)
            .Add(BasicInforSortField.Engine, s => s.Engine)
            .Add(BasicInforSortField.Publisher, s => s.Publisher)
            .Add(BasicInforSortField.GameNickname, s => s.GameNickname)
            .Add(BasicInforSortField.Tags, s => s.Tags)
            .Add(BasicInforSortField.IssueMethod, s => s.IssueMethod)
            .Add(BasicInforSortField.OfficialWebsite, s => s.OfficialWebsite)
            .Add(BasicInforSortField.SteamId, s => s.SteamId)
            .Add(BasicInforSortField.QQgroupGame, s => s.QQgroupGame);

        private static readonly SortMap<GroupInforTableModel, GroupInforSortField> ListGroupsSorting =
            new SortMap<GroupInforTableModel, GroupInforSortField>(GroupInforSortField.Id, GroupInforSortField.Id)
            .Add(GroupInforSortField.Id, s => s.Id)
            .Add(GroupInforSortField.RealId, s => s.RealId)
            .Add(GroupInforSortField.Name, s => s.Name)
            .Add(GroupInforSortField.QQgroupGroup, s => s.QQgroupGroup)
            .Add(GroupInforSortField.AnotherNameGroup, s => s.AnotherNameGroup)
            .Add(GroupInforSortField.Bilibili, s => s.Bilibili)
            .Add(GroupInforSortField.MicroBlog, s => s.MicroBlog);

        private static readonly SortMap<StaffInforTableModel, StaffInforSortField> ListStaffsSorting =
            new SortMap<StaffInforTableModel, StaffInforSortField>(StaffInforSortField.Id, StaffInforSortField.Id)
            .Add(StaffInforSortField.Id, s => s.Id)
            .Add(StaffInforSortField.RealId, s => s.RealId)
            .Add(StaffInforSortField.GameName, s => s.GameName)
            .Add(StaffInforSortField.Subcategory, s => s.Subcategory)
            .Add(StaffInforSortField.PositionOfficial, s => s.PositionOfficial)
            .Add(StaffInforSortField.NicknameOfficial, s => s.NicknameOfficial)
            .Add(StaffInforSortField.PositionGeneral, s => s.PositionGeneral)
            .Add(StaffInforSortField.Role, s => s.Role)
            .Add(StaffInforSortField.SubordinateOrganization, s => s.SubordinateOrganization);

        private static readonly SortMap<MakerInforTableModel, MakerInforSortField> ListMakersSorting =
            new SortMap<MakerInforTableModel, MakerInforSortField>(MakerInforSortField.Id, MakerInforSortField.Id)
            .Add(MakerInforSortField.Id, s => s.Id)
            .Add(MakerInforSortField.RealId, s => s.RealId)
            .Add(MakerInforSortField.Name, s => s.Name)
            .Add(MakerInforSortField.AnotherName, s => s.AnotherName)
            .Add(MakerInforSortField.Bilibili, s => s.Bilibili)
            .Add(MakerInforSortField.MicroBlog, s => s.MicroBlog)
            .Add(MakerInforSortField.Nickname, s => s.Nickname)
            .Add(MakerInforSortField.Blog, s => s.Blog)
            .Add(MakerInforSortField.Lofter, s => s.Lofter)
            .Add(MakerInforSortField.Other, s => s.Other);

        private static readonly SortMap<RoleInforTableModel, RoleInforSortField> ListRolesSorting =
            new SortMap<RoleInforTableModel, RoleInforSortField>(RoleInforSortField.Id, RoleInforSortField.Id)
            .Add(RoleInforSortField.Id, s => s.Id)
            .Add(RoleInforSortField.RealId, s => s.RealId)
            .Add(RoleInforSortField.Name, s => s.Name)
            .Add(RoleInforSortField.CV, s => s.CV)
            .Add(RoleInforSortField.AnotherNameRole, s => s.AnotherNameRole)
            .Add(RoleInforSortField.Gender, s => s.Gender)
            .Add(RoleInforSortField.FigureData, s => s.FigureData)
            .Add(RoleInforSortField.FigureSubjective, s => s.FigureSubjective)
            .Add(RoleInforSortField.Birthday, s => s.Birthday)
            .Add(RoleInforSortField.Haircolor, s => s.Haircolor)
            .Add(RoleInforSortField.Pupilcolor, s => s.Pupilcolor)
            .Add(RoleInforSortField.ClothesAccessories, s => s.ClothesAccessories)
            .Add(RoleInforSortField.Character, s => s.Character)
            .Add(RoleInforSortField.RoleIdentity, s => s.RoleIdentity)
            .Add(RoleInforSortField.BloodType, s => s.BloodType)
            .Add(RoleInforSortField.RoleHeight, s => s.RoleHeight)
            .Add(RoleInforSortField.RoleTaste, s => s.RoleTaste)
            .Add(RoleInforSortField.RoleAge, s => s.RoleAge)
            .Add(RoleInforSortField.GameName, s => s.GameName);

        private static readonly SortMap<StoreInfo, StoreTableSortField> ListStoreInfoSorting =
            new SortMap<StoreInfo, StoreTableSortField>(StoreTableSortField.Id, StoreTableSortField.Id)
            .Add(StoreTableSortField.State, s => s.State)
            .Add(StoreTableSortField.CurrencyCode, s => s.CurrencyCode)
            .Add(StoreTableSortField.CutLowest, s => s.CutLowest)
            .Add(StoreTableSortField.CutNow, s => s.CutNow)
            .Add(StoreTableSortField.EstimationOwnersMax, s => s.EstimationOwnersMax)
            .Add(StoreTableSortField.EstimationOwnersMin, s => s.EstimationOwnersMin)
            .Add(StoreTableSortField.EvaluationCount, s => s.EvaluationCount)
            .Add(StoreTableSortField.Link, s => s.Link)
            .Add(StoreTableSortField.OriginalPrice, s => s.OriginalPrice)
            .Add(StoreTableSortField.PlatformName, s => s.PlatformName)
            .Add(StoreTableSortField.PlatformType, s => s.PlatformType)
            .Add(StoreTableSortField.PlayTime, s => s.PlayTime)
            .Add(StoreTableSortField.PriceLowest, s => s.PriceLowest)
            .Add(StoreTableSortField.PriceNow, s => s.PriceNow)
            .Add(StoreTableSortField.RecommendationRate, s => s.RecommendationRate)
            .Add(StoreTableSortField.UpdateTime, s => s.UpdateTime)
            .Add(StoreTableSortField.UpdateType, s => s.UpdateType)
            .Add(StoreTableSortField.Name, s => s.Name)
            .Add(StoreTableSortField.Id, s => s.Id);

        private static readonly SortMap<GameScoreTableModel, GameScoreSortField> ListGameScoresSorting =
            new SortMap<GameScoreTableModel, GameScoreSortField>(GameScoreSortField.Id, GameScoreSortField.Id)
            .Add(GameScoreSortField.Id, s => s.Id)
            .Add(GameScoreSortField.GameId, s => s.GameId)
            .Add(GameScoreSortField.GameName, s => s.GameName)
            .Add(GameScoreSortField.AllCVSocre, s => s.AllCVSocre)
            .Add(GameScoreSortField.AllSystemSocre, s => s.AllSystemSocre)
            .Add(GameScoreSortField.AllShowSocre, s => s.AllShowSocre)
            .Add(GameScoreSortField.AllPaintSocre, s => s.AllPaintSocre)
            .Add(GameScoreSortField.AllScriptSocre, s => s.AllScriptSocre)
            .Add(GameScoreSortField.AllMusicSocre, s => s.AllMusicSocre)
            .Add(GameScoreSortField.AllTotalSocre, s => s.AllTotalSocre)
            .Add(GameScoreSortField.FilterCVSocre, s => s.FilterCVSocre)
            .Add(GameScoreSortField.FilterSystemSocre, s => s.FilterSystemSocre)
            .Add(GameScoreSortField.FilterShowSocre, s => s.FilterShowSocre)
            .Add(GameScoreSortField.FilterPaintSocre, s => s.FilterPaintSocre)
            .Add(GameScoreSortField.FilterScriptSocre, s => s.FilterScriptSocre)
            .Add(GameScoreSortField.FilterMusicSocre, s => s.FilterMusicSocre)
            .Add(GameScoreSortField.FilterTotalSocre, s => s.FilterTotalSocre);


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
        public async Task<QueryResultModel<BasicInforTableModel, BasicInforSortField>> ListGames(QueryParameterModel<BasicInforSortField> model)
        {
            var visibleGameIds = _entryRepository.GetAll().Where(s => s.Type == EntryType.Game && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => (long)s.Id);
            var (items, total) = await _queryService.QueryAsync<BasicInforTableModel, BasicInforSortField>(_basicInforTableModelRepository.GetAll().AsSingleQuery().Where(s => visibleGameIds.Contains(s.RealId)), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)), ListGamesSorting);

            return new QueryResultModel<BasicInforTableModel, BasicInforSortField>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


        [HttpPost]
        public async Task<QueryResultModel<GroupInforTableModel, GroupInforSortField>> ListGroups(QueryParameterModel<GroupInforSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<GroupInforTableModel, GroupInforSortField>(_groupInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)), ListGroupsSorting);

            return new QueryResultModel<GroupInforTableModel, GroupInforSortField>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<StaffInforTableModel, StaffInforSortField>> ListStaffs(QueryParameterModel<StaffInforSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<StaffInforTableModel, StaffInforSortField>(_staffInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.GameName.Contains(model.SearchText)), ListStaffsSorting);

            return new QueryResultModel<StaffInforTableModel, StaffInforSortField>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpPost]
        public async Task<QueryResultModel<MakerInforTableModel, MakerInforSortField>> ListMakers(QueryParameterModel<MakerInforSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<MakerInforTableModel, MakerInforSortField>(_makerInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)), ListMakersSorting);

            return new QueryResultModel<MakerInforTableModel, MakerInforSortField>
            {
                Items = await items.ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


        [HttpPost]
        public async Task<QueryResultModel<RoleInforTableModel, RoleInforSortField>> ListRoles(QueryParameterModel<RoleInforSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<RoleInforTableModel, RoleInforSortField>(_roleInforTableModelRepository.GetAll().AsSingleQuery(), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)), ListRolesSorting);

            return new QueryResultModel<RoleInforTableModel, RoleInforSortField>
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
                GamesCount = await _entryRepository.CountAsync(s => s.Type == EntryType.Game && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false),
                RolesCount = await _entryRepository.CountAsync(s => s.Type == EntryType.Role && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false),
                StaffsCount = await _entryRepository.CountAsync(s => s.Type == EntryType.Staff && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false),
                GroupsCount = await _entryRepository.CountAsync(s => s.Type == EntryType.ProductionGroup && s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false),
                ArticlesCount = await _articleRepository.LongCountAsync(s => s.IsHidden != true && string.IsNullOrWhiteSpace(s.Name) == false)
            };
            if (await _examineRepository.GetAll().AnyAsync())
            {
                model.LastEditTime = await _examineRepository.GetAll().MaxAsync(s => s.ApplyTime);
            }

            return model;
        }

        [HttpPost]
        public async Task<QueryResultModel<StoreInfoViewModel, StoreTableSortField>> ListStoreInfo(QueryParameterModel<StoreTableSortField> model)
        {
            var (items, total) = await _queryService.QueryAsync<StoreInfo, StoreTableSortField>(_storeInfoRepository.GetAll().AsSingleQuery().Where(s => s.State != StoreState.None && s.Entry != null && s.Entry.Type == EntryType.Game && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText)), ListStoreInfoSorting);

            return new QueryResultModel<StoreInfoViewModel, StoreTableSortField>
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
        public async Task<QueryResultModel<GameScoreTableModel, GameScoreSortField>> ListGameScores(QueryParameterModel<GameScoreSortField> model)
        {
            var visibleGameIds = _entryRepository.GetAll().Where(s => s.Type == EntryType.Game && s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => (long)s.Id);
            var (items, total) = await _queryService.QueryAsync<GameScoreTableModel, GameScoreSortField>(_gameScoreTableRepository.GetAll().AsSingleQuery().Where(s => visibleGameIds.Contains(s.GameId)), model,
              s => string.IsNullOrWhiteSpace(model.SearchText) || (s.GameName.Contains(model.SearchText)), ListGameScoresSorting);


            var ret = await items.ToListAsync();

            return new QueryResultModel<GameScoreTableModel, GameScoreSortField>
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
