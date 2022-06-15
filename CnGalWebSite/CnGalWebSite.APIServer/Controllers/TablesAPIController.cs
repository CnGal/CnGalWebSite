using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.Tables;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;
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
        private readonly IRepository<SteamInforTableModel, long> _steamInforTableModelRepository;
        private readonly IRepository<MakerInforTableModel, long> _makerInforTableModelRepository;
        private readonly IRepository<GameScoreTableModel, long> _gameScoreTableRepository;
        private readonly ITableService _tableService;
        public TablesAPIController(IRepository<BasicInforTableModel, long> basicInforTableModelRepository, IRepository<GameScoreTableModel, long> gameScoreTableRepository,
        IRepository<Article, long> articleRepository, IRepository<Entry, int> entryRepository, IRepository<Examine, long> examineRepository, IRepository<GroupInforTableModel, long> groupInforTableModelRepository,
            IRepository<StaffInforTableModel, long> staffInforTableModelRepository, IRepository<RoleInforTableModel, long> roleInforTableModelRepository, IRepository<SteamInforTableModel, long> steamInforTableModelRepository,
           IRepository<MakerInforTableModel, long> makerInforTableModelRepository, ITableService tableService)
        {
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _articleRepository = articleRepository;
            _basicInforTableModelRepository = basicInforTableModelRepository;
            _groupInforTableModelRepository = groupInforTableModelRepository;
            _staffInforTableModelRepository = staffInforTableModelRepository;
            _roleInforTableModelRepository = roleInforTableModelRepository;
            _steamInforTableModelRepository = steamInforTableModelRepository;
            _makerInforTableModelRepository=makerInforTableModelRepository;
            _gameScoreTableRepository = gameScoreTableRepository;
            _tableService = tableService;
        }

        [HttpGet]
        public async Task<ActionResult<BasicInforListViewModel>> GetBasicInforListAsync()
        {
            var model = new BasicInforListViewModel { BasicInfors = new List<BasicInforTableModel>() };
            var result = await _basicInforTableModelRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.BasicInfors = result;
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<GroupInforListViewModel>> GetGroupInforListAsync()
        {

            var model = new GroupInforListViewModel { GroupInfors = new List<GroupInforTableModel>() };
            var result = await _groupInforTableModelRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.GroupInfors = result;
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<StaffInforListViewModel>> GetStaffInforListAsync()
        {

            var model = new StaffInforListViewModel { StaffInfors = new List<StaffInforTableModel>() };
            var result = await _staffInforTableModelRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.StaffInfors = result;
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<MakerInforListViewModel>> GetMakerInforListAsync()
        {
            var model = new MakerInforListViewModel { MakerInfors = new List<MakerInforTableModel>() };
            var result = await _makerInforTableModelRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.MakerInfors = result;
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<RoleInforListViewModel>> GetRoleInforListAsync()
        {
            var model = new RoleInforListViewModel { RoleInfors = new List<RoleInforTableModel>() };
            var result = await _roleInforTableModelRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.RoleInfors = result;
            }

            return model;
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

        [HttpGet]
        public async Task<ActionResult<SteamInforListViewModel>> GetSteamInforListAsync()
        {
            var model = new SteamInforListViewModel { SteamInfors = new List<SteamInforTableModel>() };
            var result = await _steamInforTableModelRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.SteamInfors = result;
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<GameScoreListViewModel>> GetGameScoreListAsync()
        {
            var model = new GameScoreListViewModel();

            var result = await _gameScoreTableRepository.GetAll().AsNoTracking().ToListAsync();
            if (result != null)
            {
                model.GameScores = result;
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<EChartsTreeMapOptionModel>> GetGroupGameRoleTreeMapAsync()
        {
            return await _tableService.GetGroupGameRoleTreeMap();
        }
    }
}
