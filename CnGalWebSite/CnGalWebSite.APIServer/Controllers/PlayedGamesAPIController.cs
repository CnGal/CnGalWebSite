using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.PlayedGames;
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
        private readonly IAppHelper _appHelper;
        private readonly IPlayedGameService _playedGameService;

        public PlayedGamesAPIController(IPlayedGameService playedGameService, IRepository<PlayedGame, long> playedGameRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _playedGameRepository = playedGameRepository;
            _playedGameService = playedGameService;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> AddGameToPlayedList(AddGameToPlayedModel model)
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
                    EntryId = model.GameId
                });

                //刷新缓存
                //var tempCount = await _playedGameRepository.CountAsync(s => s.EntryId == model.GameId);
                //await _entryRepository.GetRangeUpdateTable().Where(s => s.Id == model.GameId).Set(s => s.PlayedCount, b => tempCount).ExecuteAsync();

                return new Result { Successful = true };
            }
            else
            {
                game.Type = model.Type;
                await _playedGameRepository.UpdateAsync(game);
                return new Result { Successful = true };
            }
        }
        [HttpPost]
        public async Task<ActionResult<Result>> DeleteGameFromPlayedList(DeleteGameFromPlayedModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var entry = await _playedGameRepository.FirstOrDefaultAsync(s => s.Id == model.Id && s.ApplicationUserId == user.Id);

            if (entry == null)
            {
                return new Result { Successful = false, Error = "不存在Id：" + model.Id + " 的游戏" };
            }

            await _playedGameRepository.DeleteAsync(entry);

            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<PagedResultDto<EntryInforTipViewModel>> GetUserPlayedGameListAsync(PagedSortedAndFilterInput input)
        {
            return await _playedGameService.GetPaginatedResult(input);
        }

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

        [HttpPost]
        public async Task<ActionResult<Result>> ScoreGameAsync(ScoreGameModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var entry = await _entryRepository.FirstOrDefaultAsync(s => s.Type == EntryType.Game && s.Id == model.GameId);
            if (entry == null)
            {
                return new Result { Successful = false, Error = "不存在Id：" + model.GameId + "的游戏" };
            }

            //查找是否已经添加
            var gameScore = await _playedGameRepository.FirstOrDefaultAsync(s => s.EntryId == model.GameId && s.ApplicationUserId == user.Id);
            if (gameScore == null)
            {
                return new Result { Successful = false, Error = "必须添加已玩列表后才能评分" };
            }

            gameScore.CVSocre = model.CVSocre;
            gameScore.ShowSocre = model.ShowSocre;
            gameScore.ScriptSocre = model.ScriptSocre;
            gameScore.PaintSocre = model.PaintSocre;
            gameScore.SystemSocre = model.SystemSocre;

            await _playedGameRepository.UpdateAsync(gameScore);

            return new Result { Successful = true };
        }

        [HttpGet]
        public async Task<ActionResult<List<GameRecordModel>>> GetPlayedGameInforAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var games = await _playedGameRepository.GetAll().AsNoTracking().Include(s => s.Entry).Where(s => s.ApplicationUserId == user.Id).ToListAsync();

            var model = new List<GameRecordModel>();

            foreach (var item in games)
            {
                model.Add(new GameRecordModel
                {
                    BriefIntroduction = item.Entry.BriefIntroduction,
                    IsInSteam = item.IsInSteam,
                    GameId = item.EntryId ?? 0,
                    GameImage = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    GameName = item.Entry.Name,
                    PlayDuration = item.PlayDuration,
                    Type = item.Type,
                });
            }

            return model;
        }

    }
}
