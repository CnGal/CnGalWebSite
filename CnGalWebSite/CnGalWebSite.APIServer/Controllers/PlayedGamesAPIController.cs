using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.PlayedGames;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.ExamineModel.PlayedGames;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Senparc.NeuChar.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/playedgame/[action]")]
    public class PlayedGamesAPIController : ControllerBase
    {
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<Examine, string> _examineRepository;
        private readonly IAppHelper _appHelper;
        private readonly ISteamInforService _steamInforService;
        private readonly IPlayedGameService _playedGameService;
        private readonly IExamineService _examineService;
        private readonly IUserService _userService;
        private readonly ILogger<PlayedGamesAPIController> _logger;
        private readonly IOperationRecordService _operationRecordService;
        private readonly IEditRecordService _editRecordService;

        public PlayedGamesAPIController(IPlayedGameService playedGameService, ISteamInforService steamInforService, IRepository<ApplicationUser, string> userRepository, 
            ILogger<PlayedGamesAPIController> logger, IOperationRecordService operationRecordService, IEditRecordService editRecordService,
        IRepository<PlayedGame, long> playedGameRepository, IAppHelper appHelper, IRepository<Entry, int> entryRepository, IExamineService examineService, IUserService userService, IRepository<Examine, string> examineRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _playedGameRepository = playedGameRepository;
            _playedGameService = playedGameService;
            _steamInforService = steamInforService;
            _userRepository = userRepository;
            
            _examineService = examineService;
            _userService = userService;
            _examineRepository=examineRepository;
            _logger = logger;
            _operationRecordService = operationRecordService;
            _editRecordService = editRecordService;
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
                    _playedGameService.UpdatePlayedGameData(game, examine);
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
                    SystemSocre = game.SystemSocre,
                    CVSocre = game.CVSocre,
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
                    SystemSocre=model.SystemSocre,
                    CVSocre=model.CVSocre,
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
                game.SystemSocre = model.SystemSocre;
                game.CVSocre = model.CVSocre;
                game.TotalSocre = model.TotalSocre;
                game.LastEditTime = DateTime.Now.ToCstTime();

                game = await _playedGameRepository.UpdateAsync(game);

            }

            try
            {
                await _operationRecordService.AddOperationRecord(OperationRecordType.Score, entry.Id.ToString(), user, model.Identification, HttpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {Name}({Id})身份识别失败", user.UserName, user.Id);
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

                //删除待审核记录
                await _examineRepository.GetAll().Where(s => s.PlayedGameId == game.Id && s.ApplicationUserId == user.Id && s.IsPassed == null && s.Operation == Operation.EditPlayedGameMain).ExecuteDeleteAsync();


                return new Result { Successful = true };
            }

            //有感想且公开

            var playedGameMain = new PlayedGameMain
            {
                PlayImpressions=model.PlayImpressions,
                ShowPublicly=model.ShowPublicly
            };

            //保存并尝试应用审核记录
            await _editRecordService.SaveAndApplyEditRecord(game, user, playedGameMain, Operation.EditPlayedGameMain, "");

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

            await _playedGameRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && model.Ids.Contains(s.EntryId)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));
            return new Result { Successful = true };
        }
        /// <summary>
        /// 公开游戏记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> ShowPubliclyGameRecord(HiddenGameRecordModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            await _playedGameRepository.GetAll().Where(s => s.ApplicationUserId == user.Id && model.Ids.Contains(s.EntryId)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.ShowPublicly, b => model.IsHidden));
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
            var games = await _playedGameRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry).ThenInclude(s => s.Tags)
                .Where(s => string.IsNullOrWhiteSpace(s.Entry.Name) == false && s.Entry.IsHidden == false)
                .Where(s => s.ApplicationUserId == id)
                .ToListAsync();

            var gameIds = games.Select(s => s.Id).ToList();
            //提前加载预览
            //获取审核记录
            var examines = await _examineRepository.GetAllListAsync(s => s.PlayedGameId!=null&& gameIds.Contains(s.PlayedGameId.Value) && s.ApplicationUserId == user.Id
              && (s.Operation == Operation.EditPlayedGameMain) && s.IsPassed == null);

            foreach(var item in examines)
            {
                _playedGameService.UpdatePlayedGameData(games.FirstOrDefault(s => s.Id == item.PlayedGameId.Value), item);
            }

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
                    SystemSocre = item.SystemSocre,
                    CVSocre = item.CVSocre,
                    IsDubbing = item.Entry.Tags.Any(s => s.Name == "无配音") == false
                });
            }

            return model;
        }

        /// <summary>
        ///
        /// 获取当前用户的所有Steam库中游戏Id
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<int>>> GetUserInSteamGameIdsAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            //获取词条
            var gameIds = await _playedGameRepository.GetAll().AsNoTracking().Where(s => s.ApplicationUserId == user.Id&&s.IsInSteam&&s.EntryId!=null).Select(s=>s.EntryId.Value).ToListAsync();
           

            return gameIds;
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
                .Include(s=>s.Tags)
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
                Game = _appHelper.GetEntryInforTipViewModel(entry),
                UserScores = entry.PlayedGames.Where(s => s.ShowPublicly).Select(s => new PlayedGameUserScoreModel
                {
                    Socres = new PlayedGameScoreModel
                    {
                        ScriptSocre = s.ScriptSocre,
                        ShowSocre = s.ShowSocre,
                        MusicSocre = s.MusicSocre,
                        PaintSocre = s.PaintSocre,
                        TotalSocre = s.TotalSocre,
                        SystemSocre = s.SystemSocre,
                        CVSocre = s.CVSocre,

                    },
                    LastEditTime = s.LastEditTime,
                    PlayImpressions = s.PlayImpressions,
                    User = _userService.GetUserInforViewModel(s.ApplicationUser, true).Result
                }).ToList(),
            };
            //判断是否有配音
            if(entry.Tags!=null&&entry.Tags.Any(s=>s.Name=="无配音"))
            {
                model.IsDubbing = false;

            }
            else
            {
                model.IsDubbing = true;
            }


            var gameScores = await _playedGameService.GetGameScores(entry.Id);
            if(gameScores!=null)
            {
                model.GameTotalScores.ScriptSocre = gameScores.AllScriptSocre;
                model.GameTotalScores.TotalSocre = gameScores.AllTotalSocre;
                model.GameTotalScores.ShowSocre = gameScores.AllShowSocre;
                model.GameTotalScores.MusicSocre = gameScores.AllMusicSocre;
                model.GameTotalScores.PaintSocre = gameScores.AllPaintSocre;
                model.GameTotalScores.SystemSocre = gameScores.AllSystemSocre;
                model.GameTotalScores.CVSocre = gameScores.AllCVSocre;

                model.GameReviewsScores.ScriptSocre = gameScores.FilterScriptSocre;
                model.GameReviewsScores.TotalSocre = gameScores.FilterTotalSocre;
                model.GameReviewsScores.ShowSocre = gameScores.FilterShowSocre;
                model.GameReviewsScores.MusicSocre = gameScores.FilterMusicSocre;
                model.GameReviewsScores.PaintSocre = gameScores.FilterPaintSocre;
                model.GameReviewsScores.SystemSocre = gameScores.FilterSystemSocre;
                model.GameReviewsScores.CVSocre = gameScores.FilterCVSocre;
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
                var examines = await _examineRepository.GetAll().Include(s => s.PlayedGame).Where(s => s.PlayedGameId==userScore.Id  && s.ApplicationUserId == user.Id
                  && (s.Operation == Operation.EditPlayedGameMain) && s.IsPassed == null).ToListAsync();

                var examine = examines.FirstOrDefault(s => s.Operation == Operation.EditPlayedGameMain);
                if (examine != null)
                {
                    _playedGameService.UpdatePlayedGameData(userScore, examine);
                }

                var current= model.UserScores.FirstOrDefault(s => s.User.Id == user.Id);
                if(current==null)
                {
                    current = new PlayedGameUserScoreModel
                    {
                        Socres = new PlayedGameScoreModel
                        {
                            ScriptSocre = userScore.ScriptSocre,
                            ShowSocre = userScore.ShowSocre,
                            MusicSocre = userScore.MusicSocre,
                            PaintSocre = userScore.PaintSocre,
                            TotalSocre = userScore.TotalSocre,
                            SystemSocre = userScore.SystemSocre,
                            CVSocre = userScore.CVSocre,

                        },
                        LastEditTime = userScore.LastEditTime,
                        PlayImpressions = userScore.PlayImpressions,
                        User = await _userService.GetUserInforViewModel(user, true)
                    };
                    model.UserScores.Add(current);
                }


                current.PlayImpressions = userScore.PlayImpressions;
                model.IsCurrentUserScorePublic = userScore.ShowPublicly;

                model.CurrentUserId = user.Id;
                model.IsCurrentUserScoreExist = true;

            }

            return model;
        }

        /// <summary>
        /// 获取词条列表
        /// </summary>
        /// <param name="input">分页信息</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListPlayedGameAloneModel>>> GetEntryListAsync(PlayedGamesPagesInfor input)
        {
            var dtos = await _playedGameService.GetPaginatedResult(input.Options, input.SearchModel);

            return dtos;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<PlayedGameUserScoreRandomModel>>> GetRandomUserScoresAsync()
        {
            var games = await _playedGameRepository.GetAll().AsNoTracking()
                .Include(s => s.ApplicationUser)
                .Include(s => s.Entry).ThenInclude(s => s.Tags)
                .OrderBy(x => EF.Functions.Random())
                .Where(s => s.ShowPublicly && s.MusicSocre != 0 && s.PaintSocre != 0 && s.SystemSocre != 0 && s.ScriptSocre != 0 && s.TotalSocre != 0 && string.IsNullOrWhiteSpace(s.PlayImpressions) == false && s.PlayImpressions.Length > ToolHelper.MinValidPlayImpressionsLength)
                .Take(30)
                .ToListAsync();
            var model = new List<PlayedGameUserScoreRandomModel>();
            foreach (var item in games)
            {

                var temp = new PlayedGameUserScoreRandomModel
                {
                    Socres = new PlayedGameScoreModel
                    {
                        ScriptSocre = item.ScriptSocre,
                        ShowSocre = item.ShowSocre,
                        MusicSocre = item.MusicSocre,
                        PaintSocre = item.PaintSocre,
                        TotalSocre = item.TotalSocre,
                        SystemSocre = item.SystemSocre,
                        CVSocre = item.CVSocre,

                    },
                    LastEditTime = item.LastEditTime,
                    PlayImpressions = item.PlayImpressions,
                    User = await _userService.GetUserInforViewModel(item.ApplicationUser, true),
                    GameId = item.Entry.Id,
                    GameName = item.Entry.Name,
                    IsDubbing = !(item.Entry.Tags != null && item.Entry.Tags.Any(s => s.Name == "无配音"))
                };

                model.Add(temp);
            }

            return model;
        }

    }
}
