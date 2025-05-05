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
        public readonly IAppHelper _appHelper;
        private readonly IQueryService _queryService;

        public ExpoAPIController(IQueryService queryService, IAppHelper appHelper, IRepository<ExpoGame, long> expoGameRepository, IRepository<ExpoTag, long> expoTagRepository, IRepository<Entry, int> entryRepository, IRepository<ExpoTask, long> expoTaskRepository, IRepository<ExpoAward, long> expoAwardRepository, IRepository<ExpoPrize, long> expoPrizeRepository)
        {
            _appHelper = appHelper;
            _queryService = queryService;
            _expoGameRepository = expoGameRepository;
            _expoTagRepository = expoTagRepository;
            _entryRepository = entryRepository;
            _expoTaskRepository = expoTaskRepository;
            _expoAwardRepository = expoAwardRepository;
            _expoPrizeRepository = expoPrizeRepository;
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
        public IAsyncEnumerable<ExpoGameViewModel> GetAllGamesAsync()
        {
            return _expoGameRepository.GetAll().AsNoTracking()
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
                 .AsAsyncEnumerable();
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

        [HttpGet]
        public async Task<ActionResult<ExpoUserTaskModel>> GetUserTask()
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user == null)
            {
                return BadRequest("找不到该用户");
            }

            var now = DateTime.Now.ToCstTime();

            var model = new ExpoUserTaskModel
            {
                IsPickUpSharedGames = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ShareGames),
                IsSharedGames = string.IsNullOrWhiteSpace(user.SteamId) == false,
                IsSignIn = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SignIn && s.Time.Date == now.Date),
                IsBooking = await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.Booking),
                LotteryCount = await _expoTaskRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).SumAsync(s => s.LotteryCount)
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

            if (model.Type == ExpoTaskType.Booking)
            {
                if (await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.Booking))
                {
                    return new Result { Successful = false, Error = "该奖励已经领取过了" };
                }
                else
                {
                    await _expoTaskRepository.InsertAsync(new ExpoTask
                    {
                        ApplicationUserId = user.Id,
                        Time = now,
                        Type = ExpoTaskType.Booking,
                        LotteryCount = 1
                    });
                }
            }
            else if (model.Type == ExpoTaskType.ShareGames)
            {
                if (await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.ShareGames))
                {
                    return new Result { Successful = false, Error = "该奖励已经领取过了" };
                }
                else if (string.IsNullOrWhiteSpace(user.SteamId))
                {
                    return new Result { Successful = false, Error = "需要先绑定Steam" };
                }
                else
                {
                    await _expoTaskRepository.InsertAsync(new ExpoTask
                    {
                        ApplicationUserId = user.Id,
                        Time = now,
                        Type = ExpoTaskType.ShareGames,
                        LotteryCount = 1
                    });
                }
            }
            else if (model.Type == ExpoTaskType.SignIn)
            {
                if (await _expoTaskRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.Type == ExpoTaskType.SignIn && s.Time.Date == now.Date))
                {
                    return new Result { Successful = false, Error = "该奖励已经领取过了" };
                }
                else
                {
                    await _expoTaskRepository.InsertAsync(new ExpoTask
                    {
                        ApplicationUserId = user.Id,
                        Time = now,
                        Type = ExpoTaskType.SignIn,
                        LotteryCount = 1
                    });
                }
            }


            return new Result { Successful = true };
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
                 .Select(s => new ExpoAwardOverviewModel
                 {
                     Id = s.Id,
                     Type = s.Type,
                     Count = s.Type == ExpoAwardType.NoEntry ? s.Count : s.Prizes.Count,
                     Image = s.Image,
                     Name = s.Name,
                     Url = s.Url,
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

            // 检查用户是否有抽奖次数
            var lotteryCount = await _expoTaskRepository.GetAll()
                .Where(s => s.ApplicationUserId == user.Id)
                .SumAsync(s => s.LotteryCount);

            if (lotteryCount <= 0)
            {
                return new Result
                {
                    Successful = false,
                    Error = "抽奖次数不足"
                };
            }

            // 获取所有奖项
            var awards = await _expoAwardRepository.GetAll().AsNoTracking()
                .Include(s => s.Prizes)
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

            // 使用一次抽奖次数
            await _expoTaskRepository.InsertAsync(new ExpoTask
            {
                ApplicationUserId = user.Id,
                Time = now,
                Type = ExpoTaskType.Lottery,
                LotteryCount = -1 // 消耗抽奖次数
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
    }
}
