using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/playedgame/[action]")]
    public class PlayedGamesAPIController : ControllerBase
    {
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IAppHelper _appHelper;
        private readonly ISteamInforService _steamInforService;
        private readonly IPlayedGameService _playedGameService;

        public PlayedGamesAPIController(IPlayedGameService playedGameService, ISteamInforService steamInforService, IRepository<ApplicationUser, string> userRepository,
        IRepository<PlayedGame, long> playedGameRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _playedGameRepository = playedGameRepository;
            _playedGameService = playedGameService;
            _steamInforService = steamInforService;
            _userRepository = userRepository;
        }
        /// <summary>
        /// 编辑游玩记录
        /// </summary>
        /// <param name="id">游戏Id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<EditGameRecordModel>> EditGameRecordAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Type == EntryType.Game && s.Id == id);
            if (entry == null)
            {
                return NotFound("不存在Id：" + id + " 的游戏");
            }

            //查找是否已经添加
            var game = await _playedGameRepository.GetAll().FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.EntryId == id);
            if (game == null)
            {

                return new EditGameRecordModel
                {
                    GameId = id
                };
            }
            else
            {
                return new EditGameRecordModel
                {
                    GameId = id,
                    Type = game.Type,
                    IsHidden = game.IsHidden,
                    PlayImpressions=game.PlayImpressions,
                };
            }
        }
        /// <summary>
        /// 编辑游玩记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> EditGameRecordAsync(EditGameRecordModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Type == EntryType.Game && s.Id == model.GameId);
            if (entry == null)
            {
                return new Result { Successful = false, Error = "不存在Id：" + model.GameId + " 的游戏" };
            }

            //查找是否已经添加
            var game = await _playedGameRepository.GetAll().FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.EntryId == model.GameId);
            if (game == null)
            {
                await _playedGameRepository.InsertAsync(new PlayedGame
                {
                    ApplicationUserId = user.Id,
                    Type=model.Type,
                    PlayImpressions=model.PlayImpressions,
                    EntryId = model.GameId,
                    IsHidden=model.IsHidden,
                });

                return new Result { Successful = true };
            }
            else
            {
                game.Type = model.Type;
                game.IsHidden = model.IsHidden;
                game.PlayImpressions = model.PlayImpressions;

                await _playedGameRepository.UpdateAsync(game);
                return new Result { Successful = true };
            }
        }
        /// <summary>
        /// 删除游戏记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> HiddenGameRecord(HiddenGameRecordModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            await _playedGameRepository.GetRangeUpdateTable().Where(s => s.ApplicationUserId == user.Id && model.Ids.Contains(s.EntryId)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
            return new Result { Successful = true };
        }

        /// <summary>
        /// 获取当前用户的某个游戏记录
        /// </summary>
        /// <param name="id">游戏词条Id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayedGameInforModel>> GetPlayedGameInforAsync(int id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取记录
            var record = await _playedGameRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.EntryId == id && s.ApplicationUserId == user.Id);
            if (record == null)
            {
                return new PlayedGameInforModel
                {
                    GameId = id,
                    Type = null
                };
            }

            var model = new PlayedGameInforModel
            {
                IsInSteam = record.IsInSteam,
                GameId = record.EntryId ?? 0,
                PlayDuration = record.PlayDuration,
                Type = record.Type,
            };

            return model;
        }


        /// <summary>
        ///
        /// 获取当前用户的游戏记录
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<GameRecordViewModel>>> GetUserGameRecordsAsync(string id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var objectUser = await _userRepository.FirstOrDefaultAsync(s => s.Id == id);

            if (objectUser.IsShowGameRecord == false && id != user.Id)
            {
                return NotFound("该用户的游玩记录未公开");
            }

            //获取词条
            var games = await _playedGameRepository.GetAll().AsNoTracking().Include(s => s.Entry).Where(s => s.ApplicationUserId == id).ToListAsync();

            var model = new List<GameRecordViewModel>();

            foreach (var item in games.OrderByDescending(s=>s.PlayDuration))
            {
                model.Add(new GameRecordViewModel
                {
                    GameBriefIntroduction = item.Entry.BriefIntroduction,
                    IsInSteam = item.IsInSteam,
                    GameId = item.EntryId ?? 0,
                    GameImage = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    GameName = item.Entry.Name,
                    PlayDuration = item.PlayDuration,
                    Type = item.Type,
                    PlayImpressions=item.PlayImpressions,
                    IsHidden = item.IsHidden,
                });
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<Result>> RefreshPlayedGameSteamInfor()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if(await _steamInforService.UpdateUserSteam(user)==false)
            {
                return new Result { Successful = false, Error = "无法获取Steam信息" };
            }

            return new Result { Successful = true };
        }

    }
}
