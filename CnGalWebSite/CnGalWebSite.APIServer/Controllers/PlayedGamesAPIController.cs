using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Search;
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
                return new Result { Successful = false, Error = "不存在Id：" + model.GameId + "的游戏" };
            }

            //查找是否已经添加
            if (await _playedGameRepository.LongCountAsync(s => s.ApplicationUserId == user.Id && s.EntryId == model.GameId) == 0)
            {
                await _playedGameRepository.InsertAsync(new PlayedGame
                {
                    ApplicationUserId = user.Id,
                    EntryId = model.GameId
                });

                //刷新缓存
                var tempCount = await _playedGameRepository.CountAsync(s => s.EntryId == model.GameId);
                await _entryRepository.GetRangeUpdateTable().Where(s => s.Id == model.GameId).Set(s => s.PlayedCount, b => tempCount).ExecuteAsync();

                return new Result { Successful = true };
            }
            else
            {
                return new Result { Successful = false, Error = "用户列表已有该游戏" };
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<PagedResultDto<EntryInforTipViewModel>> GetUserPlayedGameListAsync(PagedSortedAndFilterInput input)
        {
            return await _playedGameService.GetPaginatedResult(input);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayedGameInforModel>> GetPlayedGameInforAsync(int id)
        {
            //获取词条
            var entry = await _entryRepository.GetAll().AsNoTracking().Include(s => s.PlayedGames).FirstOrDefaultAsync(s => s.Id == id);
            if (entry == null)
            {
                return NotFound();
            }

            var model = new PlayedGameInforModel
            {
                IsCurrentUserPlayed = false,
                PlayedCount = entry.PlayedCount,
                Id = id,
                IsScoreEffective = false,
                IsCurrentUserScored = false
            };
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user != null)
            {
                var item = entry.PlayedGames.FirstOrDefault(s => s.ApplicationUserId == user.Id);
                if (item != null)
                {
                    model.IsCurrentUserPlayed = true;
                    //判断是否评分
                    if (item.IsScored == true)
                    {
                        model.IsCurrentUserScored = true;
                        model.CVSocreCurrent = item.CVSocre;
                        model.ShowSocreCurrent = item.ShowSocre;
                        model.ScriptSocreCurrent = item.ScriptSocre;
                        model.SystemSocreCurrent = item.SystemSocre;
                        model.PaintSocreCurrent = item.PaintSocre;
                    }
                }
            }

            //计算分数
            //取五个维度的最低分 为基准 分数减去基准分+1后是比例分
            //真实 分数直接取非零的数
            var scoreList = entry.PlayedGames.Where(s => s.IsScored == true);

            //判断分数是否过少不进行计算
            if (scoreList.Count() > 3)
            {
                model.IsScoreEffective = true;

                //先计算平均分
                model.CVSocreAverage = scoreList.Average(s => s.CVSocre);
                model.ShowSocreAverage = scoreList.Average(s => s.ShowSocre);
                model.PaintSocreAverage = scoreList.Average(s => s.PaintSocre);
                model.SystemSocreAverage = scoreList.Average(s => s.SystemSocre);
                model.ScriptSocreAverage = scoreList.Average(s => s.ScriptSocre);

                //求最低分
                var min = (new double[] { model.CVSocreAverage, model.ShowSocreAverage, model.PaintSocreAverage, model.SystemSocreAverage, model.ScriptSocreAverage }).Min() - 0.1;
                var proportion = 10;
                model.CVSocreProportion = (model.CVSocreAverage - min) * proportion;
                model.ShowSocreProportion = (model.ShowSocreAverage - min) * proportion;
                model.PaintSocreProportion = (model.PaintSocreAverage - min) * proportion;
                model.SystemSocreProportion = (model.SystemSocreAverage - min) * proportion;
                model.ScriptSocreProportion = (model.ScriptSocreAverage - min) * proportion;
            }
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
    }
}
