using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Senparc.NeuChar.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/playedgame/[action]")]
    public class PlayedGamesAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Examine, string> _examineRepository;
        private readonly IAppHelper _appHelper;
        private readonly ISteamInforService _steamInforService;
        private readonly IPlayedGameService _playedGameService;
        private readonly IExamineService _examineService;
        private readonly IUserService _userService;

        public PlayedGamesAPIController(IPlayedGameService playedGameService, ISteamInforService steamInforService, IRepository<ApplicationUser, string> userRepository, UserManager<ApplicationUser> userManager,
        IRepository<PlayedGame, long> playedGameRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IExamineService examineService, IUserService userService, IRepository<Examine, string> examineRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _playedGameRepository = playedGameRepository;
            _playedGameService = playedGameService;
            _steamInforService = steamInforService;
            _userRepository = userRepository;
            _userManager = userManager;
            _examineService = examineService;
            _userService = userService;
            _examineRepository=examineRepository;
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
                //查询是否有待审核的请求

                //获取审核记录
                var examines = await _examineRepository.GetAllListAsync(s => s.PlayedGameId == game.Id && s.ApplicationUserId == user.Id
                  && (s.Operation == Operation.EditPlayedGameMain) && s.IsPassed == null);

                var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditPlayedGameMain);
                if (examine != null)
                {
                    _playedGameService.UpdateArticleData(game, examine);
                }
                return new EditGameRecordModel
                {
                    GameId = id,
                    Type = game.Type,
                    IsHidden = game.IsHidden,
                    ShowPublicly = game.ShowPublicly,
                    PlayImpressions = game.PlayImpressions,
                    ScriptSocre = game.ScriptSocre,
                    ShowSocre = game.ShowSocre,
                    MusicSocre = game.MusicSocre,
                    PaintSocre = game.PaintSocre,
                    TotalSocre = game.TotalSocre,
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
                game = await _playedGameRepository.InsertAsync(new PlayedGame
                {
                    ApplicationUserId = user.Id,
                    Type = model.Type,
                    EntryId = model.GameId,
                    IsHidden = model.IsHidden,

                    ScoreTime = DateTime.Now.ToCstTime(),
                    LastEditTime = DateTime.Now.ToCstTime(),

                    ScriptSocre = model.ScriptSocre,
                    ShowSocre = model.ShowSocre,
                    MusicSocre = model.MusicSocre,
                    PaintSocre=model.PaintSocre,
                    TotalSocre=model.TotalSocre,             
                });

            }
            else
            {
                game.Type = model.Type;
                game.IsHidden = model.IsHidden;
                game.ScriptSocre = model.ScriptSocre;
                game.ShowSocre = model.ShowSocre;
                game.MusicSocre = model.MusicSocre;
                game.PaintSocre = model.PaintSocre;
                game.TotalSocre = model.TotalSocre;
                game.LastEditTime = DateTime.Now.ToCstTime();

                game = await _playedGameRepository.UpdateAsync(game);

            }

            if(game.ShowPublicly == model.ShowPublicly && model.PlayImpressions == game.PlayImpressions)
            {
                return new Result { Successful = true };
            }

            //不公开或没有感想 都可以直接保存

            if  (model.ShowPublicly == false || string.IsNullOrWhiteSpace(model.PlayImpressions))
            {
                game.ShowPublicly = model.ShowPublicly;
                game.PlayImpressions = model.PlayImpressions;

                game = await _playedGameRepository.UpdateAsync(game);

                return new Result { Successful = true };
            }

            //有感想且公开

            var playedGameMain = new PlayedGameMain
            {
                PlayImpressions=model.PlayImpressions,
                ShowPublicly=model.ShowPublicly
            };
            //序列化JSON
            var resulte = "";
            using (TextWriter text = new StringWriter())
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(text, playedGameMain);
                resulte = text.ToString();
            }

            //判断是否是管理员
            if (await _userManager.IsInRoleAsync(user, "Editor") == true)
            {
                await _examineService.ExamineEditPlayedGameMainAsync(game, playedGameMain);
                await _examineService.UniversalEditPlayedGameExaminedAsync(game, user, true, resulte, Operation.EditPlayedGameMain, "");
                await _appHelper.AddUserContributionValueAsync(user.Id, game.Id, Operation.EditPlayedGameMain);

            }
            else
            {
                await _examineService.UniversalEditPlayedGameExaminedAsync(game, user, false, resulte, Operation.EditPlayedGameMain, "");
            }

            return new Result { Successful = true };
        }
        /// <summary>
        /// 折叠游戏记录
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

            foreach (var item in games.OrderByDescending(s => s.PlayDuration))
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
                    PlayImpressions = objectUser.Id == user.Id ? item.PlayImpressions : "",
                    IsHidden = item.IsHidden,
                    MusicSocre = item.MusicSocre,
                    ScriptSocre = item.ScriptSocre,
                    ShowSocre = item.ShowSocre,
                    PaintSocre = item.PaintSocre,
                    TotalSocre = item.TotalSocre,
                    ShowPublicly = item.ShowPublicly,
                });
            }

            return model;
        }

        [HttpGet]
        public async Task<ActionResult<Result>> RefreshPlayedGameSteamInfor()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (await _steamInforService.UpdateUserSteam(user) == false)
            {
                return new Result { Successful = false, Error = "无法获取Steam信息" };
            }

            return new Result { Successful = true };
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayedGameOverviewModel>> GetPlayedGameOverview(int id)
        {
            var entry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.PlayedGames).ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entry == null)
            {
                return NotFound("未找到该词条");
            }
            if (entry.Type != EntryType.Game)
            {
                return NotFound("只有游戏才有评分");
            }



            var model = new PlayedGameOverviewModel
            {
                Game = await _appHelper.GetEntryInforTipViewModel(entry),
                UserScores = entry.PlayedGames.Where(s => s.ShowPublicly).Select(s => new PlayedGameUserScoreModel
                {
                    Socres = new PlayedGameScoreModel
                    {
                        ScriptSocre = s.ScriptSocre,
                        ShowSocre = s.ShowSocre,
                        MusicSocre = s.MusicSocre,
                        PaintSocre = s.PaintSocre,
                        TotalSocre = s.TotalSocre,

                    },
                    LastEditTime = s.LastEditTime,
                    PlayImpressions = s.PlayImpressions,
                    User = _userService.GetUserInforViewModel(s.ApplicationUser, true).Result
                }).ToList(),
            };

            if (model.UserScores.Count != 0)
            {
                model.GameTotalScores.ScriptSocre = model.UserScores.Average(s => s.Socres.ScriptSocre);
                model.GameTotalScores.ShowSocre = model.UserScores.Average(s => s.Socres.ShowSocre);
                model.GameTotalScores.MusicSocre = model.UserScores.Average(s => s.Socres.MusicSocre);
                model.GameTotalScores.PaintSocre = model.UserScores.Average(s => s.Socres.PaintSocre);
                model.GameTotalScores.TotalSocre = model.UserScores.Average(s => s.Socres.TotalSocre);
            }
            var reviews = model.UserScores.Where(s => string.IsNullOrWhiteSpace(s.PlayImpressions) == false && s.PlayImpressions.Length > 50);
            if (reviews.Any())
            {
                model.GameReviewsScores.ScriptSocre = reviews.Average(s => s.Socres.ScriptSocre);
                model.GameReviewsScores.ShowSocre = reviews.Average(s => s.Socres.ShowSocre);
                model.GameReviewsScores.MusicSocre = reviews.Average(s => s.Socres.MusicSocre);
                model.GameReviewsScores.PaintSocre = reviews.Average(s => s.Socres.PaintSocre);
                model.GameReviewsScores.TotalSocre = reviews.Average(s => s.Socres.TotalSocre);
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user==null)
            {
                return model;
            }

            var userScore = await _playedGameRepository.GetAll().FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id && s.EntryId == entry.Id);
            if (userScore != null)
            {
                //获取审核记录
                var examines = await _examineRepository.GetAll().Include(s => s.PlayedGame).Where(s => s.PlayedGame.EntryId == entry.Id && s.ApplicationUserId == user.Id
                  && (s.Operation == Operation.EditPlayedGameMain) && s.IsPassed == null).ToListAsync();

                var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditPlayedGameMain);
                if (examine != null)
                {
                    _playedGameService.UpdateArticleData(userScore, examine);
                }

                model.MyScores = new PlayedGameUserScoreModel
                {
                    Socres = new PlayedGameScoreModel
                    {
                        ScriptSocre = userScore.ScriptSocre,
                        ShowSocre = userScore.ShowSocre,
                        MusicSocre = userScore.MusicSocre,
                        PaintSocre = userScore.PaintSocre,
                        TotalSocre = userScore.TotalSocre,
                    },
                    LastEditTime = userScore.LastEditTime,
                    PlayImpressions = userScore.PlayImpressions,
                    User = await _userService.GetUserInforViewModel(user, true)

                };

                model.UserScores.RemoveAll(s => s.User.Id == user.Id);
                model.MyRecordPublic = userScore.ShowPublicly;
                model.MyRecordExist = true;
            }

            return model;
        }

    }
}
