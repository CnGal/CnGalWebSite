using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Almanacs;
using CnGalWebSite.DataModel.ViewModel.Expo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReverseMarkdown.Converters;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [Route("api/expo/[action]")]
    [ApiController]
    public class ExpoAPIController : ControllerBase
    {
        public readonly IRepository<ExpoGame, long> _expoGameRepository;
        public readonly IRepository<ExpoTag, long> _expoTagRepository;
        public readonly IRepository<ExpoTask, long> _expoTaskRepository;
        public readonly IRepository<Entry, int> _entryRepository;
        public readonly IRepository<ExpoAward, long> _expoAwardRepository;
        public readonly IRepository<ExpoPrize, long> _expoPrizeRepository;
        public readonly IRepository<QuestionnaireResponse, long> _questionnaireResponseRepository;
        public readonly IRepository<PlayedGame, long> _playedGameRepository;
        public readonly IRepository<ApplicationUser, string> _userRepository;
        public readonly IRepository<LotteryUser, long> _lotteryUserRepository;
        public readonly IAppHelper _appHelper;
        private readonly IQueryService _queryService;

        public ExpoAPIController(IQueryService queryService, IAppHelper appHelper, IRepository<ExpoGame, long> expoGameRepository, IRepository<ExpoTag, long> expoTagRepository, IRepository<Entry, int> entryRepository, IRepository<ExpoTask, long> expoTaskRepository, IRepository<ExpoAward, long> expoAwardRepository, IRepository<ExpoPrize, long> expoPrizeRepository, IRepository<QuestionnaireResponse, long> questionnaireResponseRepository, IRepository<PlayedGame, long> playedGameRepository, IRepository<ApplicationUser, string> userRepository, IRepository<LotteryUser, long> lotteryUserRepository)
        {
            _appHelper = appHelper;
            _queryService = queryService;
            _expoGameRepository = expoGameRepository;
            _expoTagRepository = expoTagRepository;
            _entryRepository = entryRepository;
            _expoTaskRepository = expoTaskRepository;
            _expoAwardRepository = expoAwardRepository;
            _expoPrizeRepository = expoPrizeRepository;
            _questionnaireResponseRepository = questionnaireResponseRepository;
            _playedGameRepository = playedGameRepository;
            _userRepository = userRepository;
            _lotteryUserRepository = lotteryUserRepository;
        }

        #region 游戏

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ExpoGameViewModel>> GetGameAsync([FromQuery] long id)
        {
            var item = await _expoGameRepository.GetAll().AsNoTracking()
                .Include(s => s.Game)
                .Include(s => s.Tags)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ExpoGameViewModel
            {
                Id = item.Id,
                GameId = item.GameId,
                GameName = item.Game.DisplayName,
                Image = item.Game.MainPicture,
                Priority = item.Priority,
                Hidden = item.Hidden,
                Tags = item.Tags.Select(s => new ExpoGameTagOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToList()
            };

            return model;
        }


        [AllowAnonymous]
        [HttpGet]
        public async IAsyncEnumerable<ExpoGameViewModel> GetAllGamesAsync()
        {
            var games = await _expoGameRepository.GetAll().AsNoTracking()
                 .Include(s => s.Game)
                 .Include(s => s.Tags)
                 .Where(s => s.Game != null)
                 .Select(s => new ExpoGameViewModel
                 {
                     Id = s.Id,
                     GameId = s.GameId,
                     GameName = s.Game.DisplayName,
                     Image = s.Game.MainPicture,
                     Priority = s.Priority,
                     Hidden = s.Hidden,
                     Tags = s.Tags.Select(s => new ExpoGameTagOverviewModel
                     {
                         Id = s.Id,
                         Name = s.Name,
                     }).ToList(),
                     Url = s.Game.Releases.Any(s => s.PublishPlatformType == PublishPlatformType.Steam) ? $"https://store.steampowered.com/app/{s.Game.Releases.Where(s => s.PublishPlatformType == PublishPlatformType.Steam).OrderBy(s => s.Type).First().Link}" : $"https://www.cngal.org/entries/index/{s.Game.Id}"
                 })
                 .ToListAsync();

            // 在内存中进行随机排序
            var random = new Random();
            var shuffledGames = games.OrderBy(x => random.Next()).ToList();

            foreach (var game in shuffledGames)
            {
                yield return game;
            }
        }

        [HttpGet]
        public async Task<ActionResult<ExpoGameEditModel>> EditGameAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new ExpoGameEditModel();
            }

            var item = await _expoGameRepository.GetAll()
                 .Include(s => s.Game)
                .Include(s => s.Tags)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ExpoGameEditModel
            {
                Id = id,
                GameId = item.GameId,
                GameName = item.Game.Name,
                Priority = item.Priority,
                Hidden = item.Hidden,
                Tags = item.Tags.Select(s => new ExpoGameTagOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                }).ToList()
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditGameAsync(ExpoGameEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Successful)
            {
                return vail;
            }

            // 检查游戏是否重复
            if (await _expoGameRepository.AnyAsync(s => s.Game.Name == model.GameName && s.Id != model.Id))
            {
                return new Result { Successful = false, Error = "游戏重复" };
            }

            ExpoGame item = null;
            if (model.Id == 0)
            {
                item = await _expoGameRepository.InsertAsync(new ExpoGame
                {
                    Priority = model.Priority,
                });
                model.Id = item.Id;
                _expoGameRepository.Clear();
            }

            item = await _expoGameRepository.GetAll()
                 .Include(s => s.Game)
                .Include(s => s.Tags)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (item == null)
            {
                return new Result { Successful = false, Error = "项目不存在" };
            }

            item.Priority = model.Priority;
            item.Hidden = model.Hidden;

            //Tags
            var tags = new List<ExpoTag>();
            foreach (var info in model.Tags)
            {
                var tag = await _expoTagRepository.GetAll().FirstOrDefaultAsync(s => s.Name == info.Name);
                if (tag != null)
                {
                    tags.Add(tag);
                }
            }
            item.Tags.RemoveAll(s => tags.Select(s => s.Id).Contains(s.Id) == false);
            item.Tags.AddRange(tags.Where(s => item.Tags.Select(s => s.Id).Contains(s.Id) == false));

            //Game
            if (string.IsNullOrWhiteSpace(model.GameName) == false)
            {
                var game = await _entryRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Name == model.GameName);
                if (game != null)
                {
                    item.GameId = game.Id;
                }
                else
                {
                    item.GameId = null;
                }
            }
            else
            {
                item.GameId = null;
            }

            await _expoGameRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<QueryResultModel<ExpoGameOverviewModel>> ListGame(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ExpoGame, long>(_expoGameRepository.GetAll().AsSingleQuery().Include(s => s.Tags).Include(s => s.Game), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Game != null && s.Game.Name.Contains(model.SearchText)));

            return new QueryResultModel<ExpoGameOverviewModel>
            {
                Items = await items.Select(s => new ExpoGameOverviewModel
                {
                    Id = s.Id,
                    GameId = s.Game.Id,
                    GameName = s.Game.DisplayName,
                    Priority = s.Priority,
                    Hidden = s.Hidden,
                    Tags = s.Tags.Select(s => new ExpoGameTagOverviewModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                    }).ToList()
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        #endregion

        #region 标签

        [AllowAnonymous]
        [HttpGet]
        public IAsyncEnumerable<ExpoTagViewModel> GetAllTagsAsync()
        {
            return _expoTagRepository.GetAll().AsNoTracking()
                 .Include(s => s.Games)
                 .Select(s => new ExpoTagViewModel
                 {
                     Id = s.Id,
                     Priority = s.Priority,
                     Name = s.Name,
                 })
                 .AsAsyncEnumerable();
        }


        [HttpGet]
        public async Task<ActionResult<ExpoTagEditModel>> EditTagAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new ExpoTagEditModel();
            }

            var item = await _expoTagRepository.GetAll()
                 .Include(s => s.Games).ThenInclude(s => s.Game)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ExpoTagEditModel
            {
                Id = id,
                Priority = item.Priority,
                Hidden = item.Hidden,
                Games = item.Games.Select(s => new ExpoGameTagOverviewModel
                {
                    Id = s.Id,
                    Name = s.Game.Name,
                }).ToList(),
                Name = item.Name,
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditTagAsync(ExpoTagEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Successful)
            {
                return vail;
            }

            // 检查是否重复
            if (await _expoTagRepository.AnyAsync(s => s.Name == model.Name && s.Id != model.Id))
            {
                return new Result { Successful = false, Error = "标签重复" };
            }

            ExpoTag item = null;
            if (model.Id == 0)
            {
                item = await _expoTagRepository.InsertAsync(new ExpoTag
                {
                    Priority = model.Priority,
                });
                model.Id = item.Id;
                _expoTagRepository.Clear();
            }

            item = await _expoTagRepository.GetAll()
                   .Include(s => s.Games).ThenInclude(s => s.Game)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (item == null)
            {
                return new Result { Successful = false, Error = "项目不存在" };
            }

            item.Priority = model.Priority;
            item.Hidden = model.Hidden;
            item.Name = model.Name;

            //Games
            var games = new List<ExpoGame>();
            foreach (var info in model.Games)
            {
                var game = await _expoGameRepository.GetAll().Include(s => s.Game).FirstOrDefaultAsync(s => s.Game.Name == info.Name);
                if (game != null)
                {
                    games.Add(game);
                }
            }
            item.Games.RemoveAll(s => games.Select(s => s.Id).Contains(s.Id) == false);
            item.Games.AddRange(games.Where(s => item.Games.Select(s => s.Id).Contains(s.Id) == false).Select(s => new ExpoGame
            {
                Id = s.Id,
                Priority = s.Priority,
                Hidden = s.Hidden,
                Game = s.Game,
                GameId = s.GameId,
                Tags = s.Tags,
            }));

            await _expoTagRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<QueryResultModel<ExpoTagOverviewModel>> ListTag(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ExpoTag, long>(_expoTagRepository.GetAll().AsSingleQuery().Include(s => s.Games).ThenInclude(s => s.Game), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name != null && s.Name.Contains(model.SearchText)));

            return new QueryResultModel<ExpoTagOverviewModel>
            {
                Items = await items.Select(s => new ExpoTagOverviewModel
                {
                    Id = s.Id,
                    Priority = s.Priority,
                    Hidden = s.Hidden,
                    Name = s.Name,
                    Games = s.Games.Select(s => new ExpoGameTagOverviewModel
                    {
                        Id = s.Id,
                        Name = s.Game.Name,
                    }).ToList()
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        #endregion

        #region 任务

        /// <summary>
        /// 获取任务对应的点数
        /// </summary>
        /// <param name="taskType">任务类型</param>
        /// <returns>点数</returns>
        private int GetTaskPoints(ExpoTaskType taskType)
        {
            return taskType switch
            {
                ExpoTaskType.SignIn => 20,
                ExpoTaskType.Booking => 50,
                ExpoTaskType.ShareGames => 50,
                ExpoTaskType.Lottery => -100, // 抽奖消耗100点
                ExpoTaskType.Survey => 100,
                ExpoTaskType.RateGame => 20,
                ExpoTaskType.BindQQ => 20,
                ExpoTaskType.ChangeAvatar => 20,
                ExpoTaskType.ChangeSignature => 20,
                ExpoTaskType.SaveGGeneration => 20,
                ExpoTaskType.LotteryNumber => 100,
                _ => 0
            };
        }

        [HttpGet]
        public async Task<ActionResult<ExpoUserTaskModel>> GetUserTask()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

           
            // 获取用户的所有任务
            var userTasks = await _expoTaskRepository.GetAll()
                .Where(s => s.ApplicationUserId == user.Id)
                .ToListAsync();

            // 计算签到天数（限制最多5天，按不同日期计算）
            var signInDays = Math.Min(5, userTasks.Where(s => s.Type == ExpoTaskType.SignIn)
                .Select(s => s.Time.Date)
                .Distinct()
                .Count());

            // 计算总点数
            var totalPoints = 0;

            // 获取所有不同日期的签到任务，按日期排序，最多取前5天
            var signInDates = userTasks.Where(s => s.Type == ExpoTaskType.SignIn)
                .Select(s => s.Time.Date)
                .Distinct()
                .OrderBy(date => date)
                .Take(5)
                .ToList();

            foreach (var task in userTasks)
            {
                var points = GetTaskPoints(task.Type);
                if (task.Type == ExpoTaskType.SignIn)
                {
                    // 签到任务：前5天每天20点，超过5天的不算点数
                    if (signInDates.Contains(task.Time.Date))
                    {
                        totalPoints += points;
                    }
                }
                else
                {
                    totalPoints += points;
                }
            }

            // 因为数据库里是+8的时区，所以这里需要用+8的时区去比较。但是如果是把数据拿出来，就不需要+8了
            // 之前留的大坑orz
            var now = DateTime.Now.ToCstTime().AddHours(8);

            var model = new ExpoUserTaskModel
            {
                IsPickUpSharedGames = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ShareGames),
                IsSharedGames = string.IsNullOrWhiteSpace(user.SteamId) == false,
                IsSignIn = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SignIn && s.Time.Date == now.Date),
                IsBooking = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.Booking),
                SignInDays = signInDays,
                IsSurvey = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.Survey),
                IsRateGame = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.RateGame),
                IsBindQQ = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.BindQQ),
                IsChangeAvatar = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ChangeAvatar),
                IsChangeSignature = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ChangeSignature),
                IsSaveGGeneration = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SaveGGeneration),
                IsLotteryNumber = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.LotteryNumber),
                TotalPoints = Math.Max(0, totalPoints) // 确保点数不为负
            };

            return model;
        }

        [HttpPost]
        public async Task<ActionResult<Result>> UserFinshTask(ExpoFinshTaskModel model)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var now = DateTime.Now.ToCstTime();
            var points = GetTaskPoints(model.Type);

            // 检查任务是否已经完成过了
            var taskExists = false;
            var errorMessage = "";

            switch (model.Type)
            {
                case ExpoTaskType.Booking:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.Booking);
                    errorMessage = "预约直播奖励已经领取过了";
                    break;

                case ExpoTaskType.ShareGames:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ShareGames);
                    if (string.IsNullOrWhiteSpace(user.SteamId))
                    {
                        return new Result { Successful = false, Error = "需要先绑定Steam" };
                    }
                    errorMessage = "分享游戏库奖励已经领取过了";
                    break;

                case ExpoTaskType.SignIn:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SignIn && s.Time.Date == now.AddHours(8).Date);
                    // 检查签到天数是否已达上限
                    var signInCount = await _expoTaskRepository.CountAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SignIn);
                    if (signInCount >= 5)
                    {
                        return new Result { Successful = false, Error = "签到奖励已达上限（5天）" };
                    }
                    if (taskExists)
                    {
                        return new Result { Successful = false, Error = "今天已经签到了，明天再来吧" };
                    }
                    errorMessage = "今日已经签到过了";
                    break;

                case ExpoTaskType.Survey:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.Survey);
                    if (!taskExists)
                    {
                        // 验证用户是否真的完成了问卷(ID=1)
                        var hasCompletedSurvey = await CheckUserCompletedSurvey(user.Id, 1);
                        if (!hasCompletedSurvey)
                        {
                            return new Result { Successful = false, Error = "请先完成问卷后再领取奖励" };
                        }
                    }
                    errorMessage = "填写问卷奖励已经领取过了";
                    break;

                case ExpoTaskType.RateGame:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.RateGame);
                    if (!taskExists)
                    {
                        // 验证用户是否真的有游戏评分记录
                        var hasGameRating = await CheckUserHasGameRating(user.Id);
                        if (!hasGameRating)
                        {
                            return new Result { Successful = false, Error = "请先给游戏评分后再领取奖励" };
                        }
                    }
                    errorMessage = "给游戏评分奖励已经领取过了";
                    break;

                case ExpoTaskType.BindQQ:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.BindQQ);
                    if (!taskExists)
                    {
                        // 验证用户是否真的绑定了群聊QQ
                        var hasBoundQQ = await CheckUserHasBoundQQ(user.Id);
                        if (!hasBoundQQ)
                        {
                            return new Result { Successful = false, Error = "请先绑定群聊QQ后再领取奖励" };
                        }
                    }
                    errorMessage = "绑定群聊QQ奖励已经领取过了";
                    break;

                case ExpoTaskType.ChangeAvatar:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ChangeAvatar);
                    if (!taskExists)
                    {
                        // 验证用户是否真的更换了默认头像
                        var hasChangedAvatar = await CheckUserHasChangedAvatar(user.Id);
                        if (!hasChangedAvatar)
                        {
                            return new Result { Successful = false, Error = "请先更换默认头像后再领取奖励" };
                        }
                    }
                    errorMessage = "更换默认头像奖励已经领取过了";
                    break;

                case ExpoTaskType.ChangeSignature:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ChangeSignature);
                    if (!taskExists)
                    {
                        // 验证用户是否真的更换了默认签名
                        var hasChangedSignature = await CheckUserHasChangedSignature(user.Id);
                        if (!hasChangedSignature)
                        {
                            return new Result { Successful = false, Error = "请先更换默认签名后再领取奖励" };
                        }
                    }
                    errorMessage = "更换默认签名奖励已经领取过了";
                    break;

                case ExpoTaskType.SaveGGeneration:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SaveGGeneration);
                    errorMessage = "填写国G世代奖励已经领取过了";
                    break;

                case ExpoTaskType.LotteryNumber:
                    taskExists = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.LotteryNumber);
                    if (!taskExists)
                    {
                        // 验证用户是否真的参与了抽奖（ID=38）
                        var hasJoinedLottery = await CheckUserHasJoinedLottery(user.Id, 38);
                        if (!hasJoinedLottery)
                        {
                            return new Result { Successful = false, Error = "请先参与十周年庆典抽奖后再领取奖励" };
                        }
                    }
                    errorMessage = "领取抽奖号码奖励已经领取过了";
                    break;

                default:
                    return new Result { Successful = false, Error = "未知的任务类型" };
            }

            if (taskExists)
            {
                return new Result { Successful = false, Error = errorMessage };
            }

            // 创建任务记录
            await _expoTaskRepository.InsertAsync(new ExpoTask
            {
                ApplicationUserId = user.Id,
                Time = now,
                Type = model.Type,
                LotteryCount = points // 使用点数系统，向下兼容
            });

            return new Result { Successful = true };
        }

        /// <summary>
        /// 检查用户是否有游戏评分记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<bool>> CheckUserGameRating()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var hasRating = await CheckUserHasGameRating(user.Id);
            return hasRating;
        }

        /// <summary>
        /// 检查用户是否绑定了群聊QQ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<bool>> CheckUserBindQQ()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var hasBoundQQ = await CheckUserHasBoundQQ(user.Id);
            return hasBoundQQ;
        }

        /// <summary>
        /// 检查用户是否更换了默认头像
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<bool>> CheckUserChangeAvatar()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var hasChangedAvatar = await CheckUserHasChangedAvatar(user.Id);
            return hasChangedAvatar;
        }

        /// <summary>
        /// 检查用户是否更换了默认签名
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<bool>> CheckUserChangeSignature()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var hasChangedSignature = await CheckUserHasChangedSignature(user.Id);
            return hasChangedSignature;
        }

        /// <summary>
        /// 检查用户是否参与了抽奖
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<bool>> CheckUserJoinedLottery()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var hasJoinedLottery = await CheckUserHasJoinedLottery(user.Id, 38); // 检查抽奖ID=38
            return hasJoinedLottery;
        }

        [HttpPost]
        public async Task<QueryResultModel<ExpoTaskOverviewModel>> ListTask(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ExpoTask, long>(_expoTaskRepository.GetAll().AsSingleQuery().Include(s => s.ApplicationUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.ApplicationUser != null && s.ApplicationUser.UserName != null && s.ApplicationUser.UserName.Contains(model.SearchText)) || (s.ApplicationUserId != null && s.ApplicationUserId.Contains(model.SearchText)));

            return new QueryResultModel<ExpoTaskOverviewModel>
            {
                Items = await items.Select(s => new ExpoTaskOverviewModel
                {
                    Id = s.Id,
                    LotteryCount = s.LotteryCount,
                    Time = s.Time,
                    Type = s.Type,
                    UserId = s.ApplicationUserId,
                    UserName = s.ApplicationUser.UserName
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        #endregion

        #region 奖项

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ExpoAwardViewModel>> GetAwardAsync([FromQuery] long id)
        {
            var item = await _expoAwardRepository.GetAll().AsNoTracking()
                .Include(s => s.Prizes).ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ExpoAwardViewModel
            {
                Id = item.Id,
                Type = item.Type,
                Count = item.Count,
                Image = item.Image,
                Name = item.Name,
                Url = item.Url,
                IsEnabled = item.IsEnabled,
                Prizes = item.Prizes.Select(s => new ExpoPrizeOverviewModel
                {
                    Id = s.Id,
                    Content = s.Content,
                    UserId = s.ApplicationUserId,
                    UserName = s.ApplicationUser?.UserName,
                    DrawTime = s.DrawTime
                }).ToList()
            };

            return model;
        }

        [AllowAnonymous]
        [HttpGet]
        public IAsyncEnumerable<ExpoAwardOverviewModel> GetAllAwardsAsync()
        {
            return _expoAwardRepository.GetAll().AsNoTracking()
                 .Include(s => s.Prizes)
                 .Where(s => s.IsEnabled) // 只返回启用的奖项
                 .Select(s => new ExpoAwardOverviewModel
                 {
                     Id = s.Id,
                     Type = s.Type,
                     Count = s.Type == ExpoAwardType.NoEntry ? s.Count : s.Prizes.Count,
                     Image = s.Image,
                     Name = s.Name,
                     Url = s.Url,
                     IsEnabled = s.IsEnabled,
                     PrizeCount = s.Prizes.Count(s => string.IsNullOrWhiteSpace(s.ApplicationUserId) == false)
                 })
                 .Where(s => s.Count > s.PrizeCount)
                 .AsAsyncEnumerable();
        }

        [HttpGet]
        public async Task<ActionResult<ExpoAwardEditModel>> EditAwardAsync([FromQuery] long id)
        {
            if (id == 0)
            {
                return new ExpoAwardEditModel();
            }

            var item = await _expoAwardRepository.GetAll()
                .Include(s => s.Prizes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new ExpoAwardEditModel
            {
                Id = id,
                Type = item.Type,
                Count = item.Count,
                Image = item.Image,
                Name = item.Name,
                Url = item.Url,
                IsEnabled = item.IsEnabled,
                Prizes = item.Prizes.Select(s => new ExpoPrizeEditModel
                {
                    Id = s.Id,
                    Content = s.Content,
                    Allocated = string.IsNullOrWhiteSpace(s.ApplicationUserId) == false
                }).ToList()
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> EditAwardAsync(ExpoAwardEditModel model)
        {
            var vail = model.Validate();
            if (!vail.Successful)
            {
                return vail;
            }

            ExpoAward item = null;
            if (model.Id == 0)
            {
                item = await _expoAwardRepository.InsertAsync(new ExpoAward
                {
                    Type = model.Type,
                    Count = model.Count,
                    Image = model.Image,
                    Name = model.Name,
                    Url = model.Url,
                    Prizes = new List<ExpoPrize>()
                });
                model.Id = item.Id;
                _expoAwardRepository.Clear();
            }

            item = await _expoAwardRepository.GetAll()
                .Include(s => s.Prizes)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (item == null)
            {
                return new Result { Successful = false, Error = "项目不存在" };
            }

            item.Type = model.Type;
            item.Count = model.Count;
            item.Image = model.Image;
            item.Name = model.Name;
            item.Url = model.Url;
            item.IsEnabled = model.IsEnabled;

            // 处理奖品
            if (model.Type == ExpoAwardType.Key)
            {
                // 删除已被删除的奖品
                var prizeIds = model.Prizes.Where(s => s.Id > 0).Select(s => s.Id).ToList();
                var deleteItems = item.Prizes.Where(s => !prizeIds.Contains(s.Id) && string.IsNullOrWhiteSpace(s.ApplicationUserId)).ToList();
                foreach (var deleteItem in deleteItems)
                {
                    item.Prizes.Remove(deleteItem);
                    await _expoPrizeRepository.DeleteAsync(deleteItem);
                }

                // 更新已有奖品
                foreach (var prize in model.Prizes.Where(s => s.Id > 0))
                {
                    var existingPrize = item.Prizes.FirstOrDefault(s => s.Id == prize.Id);
                    if (existingPrize != null)
                    {
                        existingPrize.Content = prize.Content;
                    }
                }

                // 添加新奖品
                var newPrizes = model.Prizes.Where(s => s.Id == 0).Select(s => new ExpoPrize
                {
                    Content = s.Content,
                    AwardId = item.Id,
                    DrawTime = DateTime.MinValue
                }).ToList();

                item.Prizes.AddRange(newPrizes);
            }

            await _expoAwardRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }

        [HttpPost]
        public async Task<QueryResultModel<ExpoAwardOverviewModel>> ListAward(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ExpoAward, long>(_expoAwardRepository.GetAll().Include(s => s.Prizes), model,
                s => true);

            return new QueryResultModel<ExpoAwardOverviewModel>
            {
                Items = await items.Select(s => new ExpoAwardOverviewModel
                {
                    Id = s.Id,
                    Type = s.Type,
                    Count = s.Type == ExpoAwardType.NoEntry ? s.Count : s.Prizes.Count,
                    Image = s.Image,
                    Name = s.Name,
                    Url = s.Url,
                    IsEnabled = s.IsEnabled,
                    PrizeCount = s.Prizes.Count(s => string.IsNullOrWhiteSpace(s.ApplicationUserId) == false)
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        /// <summary>
        /// 用户抽奖
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Result>> LotteryAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var now = DateTime.Now.ToCstTime();

            // 计算用户当前总点数
            var userTasks = await _expoTaskRepository.GetAll()
                .Where(s => s.ApplicationUserId == user.Id)
                .ToListAsync();

            var totalPoints = 0;

            // 获取所有不同日期的签到任务，按日期排序，最多取前5天
            var signInDates = userTasks.Where(s => s.Type == ExpoTaskType.SignIn)
                .Select(s => s.Time.Date)
                .Distinct()
                .OrderBy(date => date)
                .Take(5)
                .ToList();

            foreach (var task in userTasks)
            {
                var points = GetTaskPoints(task.Type);
                if (task.Type == ExpoTaskType.SignIn)
                {
                    // 签到任务：前5天每天20点，超过5天的不算点数
                    if (signInDates.Contains(task.Time.Date))
                    {
                        totalPoints += points;
                    }
                }
                else
                {
                    totalPoints += points;
                }
            }

            if (totalPoints < 100)
            {
                return new Result
                {
                    Successful = false,
                    Error = "点数不足，需要100点才能抽奖"
                };
            }

            // 获取所有启用的奖项
            var awards = await _expoAwardRepository.GetAll().AsNoTracking()
                .Include(s => s.Prizes)
                .Where(s => s.IsEnabled) // 只考虑启用的奖项
                .ToListAsync();

            // 检查每个奖项是否还有剩余
            var availableAwards = new List<ExpoAward>();
            foreach (var award in awards)
            {
                // 查询已分配的奖品数量
                var allocatedCount = await _expoPrizeRepository.CountAsync(s => s.AwardId == award.Id && !string.IsNullOrWhiteSpace(s.ApplicationUserId));
                var awardCount = award.Count;
                if (award.Type != ExpoAwardType.NoEntry)
                {
                    awardCount = await _expoPrizeRepository.CountAsync(s => s.AwardId == award.Id);
                }

                if (allocatedCount < awardCount)
                {
                    availableAwards.Add(award);
                }
            }

            // 没有可用奖项
            if (availableAwards.Count == 0)
            {
                return new Result
                {
                    Successful = false,
                    Error = "没有可用奖项"
                };
            }

            // 按照prize数量分配概率
            var random = new Random();
            ExpoAward selectedAward = null;

            // 获取所有可用奖项的未分配prize总数
            Dictionary<ExpoAward, int> awardPrizeCounts = new Dictionary<ExpoAward, int>();
            int totalAvailablePrizes = 0;

            foreach (var award in availableAwards)
            {
                int prizeCount = 0;
                if (award.Type == ExpoAwardType.Key)
                {
                    // 对于Key类型，计算未分配的奖品数量
                    prizeCount = award.Prizes.Count(p => string.IsNullOrWhiteSpace(p.ApplicationUserId));
                }
                else // NoEntry类型
                {
                    // 对于NoEntry类型，计算剩余可创建的奖品数量
                    var allocatedCount = await _expoPrizeRepository.CountAsync(s => s.AwardId == award.Id && !string.IsNullOrWhiteSpace(s.ApplicationUserId));
                    prizeCount = award.Count - allocatedCount;
                }

                if (prizeCount > 0)
                {
                    awardPrizeCounts.Add(award, prizeCount);
                    totalAvailablePrizes += prizeCount;
                }
            }

            // 如果没有可用的奖品
            if (totalAvailablePrizes == 0)
            {
                return new Result
                {
                    Successful = false,
                    Error = "没有可用奖项"
                };
            }

            // 按照prize数量比例选择award
            int randomNumber = random.Next(totalAvailablePrizes);
            int currentSum = 0;

            foreach (var awardEntry in awardPrizeCounts)
            {
                currentSum += awardEntry.Value;
                if (randomNumber < currentSum)
                {
                    selectedAward = awardEntry.Key;
                    break;
                }
            }

            // 如果没有选中奖项（理论上不应该发生）
            if (selectedAward == null)
            {
                return new Result
                {
                    Successful = false,
                    Error = "没有选中奖项"
                };
            }

            // 消耗100点数进行抽奖
            await _expoTaskRepository.InsertAsync(new ExpoTask
            {
                ApplicationUserId = user.Id,
                Time = now,
                Type = ExpoTaskType.Lottery,
                LotteryCount = -100 // 消耗100点数
            });

            // 处理不同类型的奖项
            if (selectedAward.Type == ExpoAwardType.Key)
            {
                // 获取未分配的奖品
                var availablePrizes = await _expoPrizeRepository.GetAll()
                    .Where(s => s.AwardId == selectedAward.Id && string.IsNullOrWhiteSpace(s.ApplicationUserId))
                    .ToListAsync();

                if (availablePrizes.Count == 0)
                {
                    // 没有可用的奖品，返回失败
                    return new Result
                    {
                        Successful = false,
                        Error = "没有可用的奖品"
                    };
                }

                // 随机选择一个奖品
                var selectedPrize = availablePrizes[random.Next(availablePrizes.Count)];

                // 分配给用户
                selectedPrize.ApplicationUserId = user.Id;
                selectedPrize.DrawTime = now;
                await _expoPrizeRepository.UpdateAsync(selectedPrize);

                // 返回成功和奖品内容
                return new Result
                {
                    Successful = true,
                    Error = selectedAward.Id.ToString()
                };
            }
            else // ExpoAwardType.NoEntry
            {
                // 查询已分配的奖品数量
                var allocatedCount = await _expoPrizeRepository.CountAsync(s => s.AwardId == selectedAward.Id && !string.IsNullOrWhiteSpace(s.ApplicationUserId));

                if (allocatedCount < selectedAward.Count)
                {
                    // 创建一个无实体奖品记录
                    var prize = new ExpoPrize
                    {
                        AwardId = selectedAward.Id,
                        ApplicationUserId = user.Id,
                        Content = "无实体奖品",
                        DrawTime = now
                    };

                    await _expoPrizeRepository.InsertAsync(prize);

                    // 返回成功
                    return new Result
                    {
                        Successful = true,
                        Error = selectedAward.Id.ToString()
                    };
                }
                else
                {
                    // 奖品已分配完，返回失败
                    return new Result
                    {
                        Successful = false,
                        Error = "奖品已分配完"
                    };
                }
            }
        }

        /// <summary>
        /// 获取用户的所有奖品
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<ExpoPrizeOverviewModel>>> GetUserPrizesAsync()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var prizes = await _expoPrizeRepository.GetAll().AsNoTracking()
                .Include(s => s.Award)
                .Where(s => s.ApplicationUserId == user.Id)
                .Select(s => new ExpoPrizeOverviewModel
                {
                    Id = s.Id,
                    Content = s.Content,
                    DrawTime = s.DrawTime,
                    UserId = s.ApplicationUserId,
                    UserName = user.UserName,
                    AwardId = s.AwardId,
                    AwardName = s.Award.Name,
                    AwardType = s.Award.Type,
                })
                .ToListAsync();

            return prizes;
        }

        #endregion

        #region 奖品

        [HttpPost]
        public async Task<QueryResultModel<ExpoPrizeOverviewModel>> ListPrize(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<ExpoPrize, long>(_expoPrizeRepository.GetAll().AsSingleQuery().Include(s => s.Award).Include(s => s.ApplicationUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) ||
                     (s.Content != null && s.Content.Contains(model.SearchText)) ||
                     (s.ApplicationUser != null && s.ApplicationUser.UserName != null && s.ApplicationUser.UserName.Contains(model.SearchText)) ||
                     (s.Award != null && s.Award.Name != null && s.Award.Name.Contains(model.SearchText)));

            return new QueryResultModel<ExpoPrizeOverviewModel>
            {
                Items = await items.Select(s => new ExpoPrizeOverviewModel
                {
                    Id = s.Id,
                    Content = s.Content,
                    UserId = s.ApplicationUserId,
                    UserName = s.ApplicationUser == null ? "" : s.ApplicationUser.UserName,
                    DrawTime = s.DrawTime,
                    AwardId = s.AwardId,
                    AwardName = s.Award.Name,
                    AwardType = s.Award.Type,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 检查用户是否完成了指定问卷
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="questionnaireId">问卷ID</param>
        /// <returns>是否完成</returns>
        private async Task<bool> CheckUserCompletedSurvey(string userId, long questionnaireId)
        {
            try
            {
                // 检查用户是否已提交问卷回答
                var response = await _questionnaireResponseRepository.GetAll()
                    .FirstOrDefaultAsync(r => r.ApplicationUserId == userId && r.QuestionnaireId == questionnaireId && r.IsCompleted);

                return response != null;
            }
            catch (Exception)
            {
                // 如果查询失败，返回false，确保安全
                return false;
            }
        }

                /// <summary>
        /// 检查用户是否有游戏评分记录
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否有评分</returns>
        private async Task<bool> CheckUserHasGameRating(string userId)
        {
            try
            {
                // 检查用户是否有有效的游戏评分记录
                // 有效评分定义：至少有3个评分项不为0，且总评分不为0
                var hasValidRating = await _playedGameRepository.GetAll()
                    .AnyAsync(r => r.ApplicationUserId == userId &&
                                 r.TotalSocre > 0 &&
                                 (new[] { r.ScriptSocre, r.ShowSocre, r.MusicSocre, r.PaintSocre, r.SystemSocre, r.CVSocre })
                                 .Count(score => score > 0) >= 3);

                return hasValidRating;
            }
            catch (Exception)
            {
                // 如果查询失败，返回false，确保安全
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否绑定了群聊QQ
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否绑定QQ</returns>
        private async Task<bool> CheckUserHasBoundQQ(string userId)
        {
            try
            {
                // 检查用户是否绑定了群聊QQ（GroupQQ > 0表示已绑定）
                var user = await _userRepository.GetAll()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                return user != null && user.GroupQQ > 0;
            }
            catch (Exception)
            {
                // 如果查询失败，返回false，确保安全
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否更换了默认头像
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否更换了头像</returns>
        private async Task<bool> CheckUserHasChangedAvatar(string userId)
        {
            try
            {
                // 检查用户是否设置了非默认头像（PhotoPath不为空且不为默认值）
                var user = await _userRepository.GetAll()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                // 头像路径不为空且不为默认头像文件名时表示已更换
                return user != null && !string.IsNullOrWhiteSpace(user.PhotoPath);
            }
            catch (Exception)
            {
                // 如果查询失败，返回false，确保安全
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否更换了默认签名
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>是否更换了签名</returns>
        private async Task<bool> CheckUserHasChangedSignature(string userId)
        {
            try
            {
                // 检查用户是否设置了非默认签名（PersonalSignature不为空）
                var user = await _userRepository.GetAll()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                // 个人签名不为空时表示已更换
                return user != null && !string.IsNullOrWhiteSpace(user.PersonalSignature) && user.PersonalSignature != "哇，这里什么都没有呢" && user.PersonalSignature != "这个人太懒了，什么也没写额(～￣▽￣)～";
            }
            catch (Exception)
            {
                // 如果查询失败，返回false，确保安全
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否参与了指定抽奖
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="lotteryId">抽奖ID</param>
        /// <returns>是否参与了抽奖</returns>
        private async Task<bool> CheckUserHasJoinedLottery(string userId, long lotteryId)
        {
            try
            {
                // 检查用户是否参与了指定的抽奖
                var hasJoined = await _lotteryUserRepository.GetAll()
                    .AsNoTracking()
                    .AnyAsync(l => l.ApplicationUserId == userId && l.LotteryId == lotteryId);

                return hasJoined;
            }
            catch (Exception)
            {
                // 如果查询失败，返回false，确保安全
                return false;
            }
        }

        #endregion
    }
}
