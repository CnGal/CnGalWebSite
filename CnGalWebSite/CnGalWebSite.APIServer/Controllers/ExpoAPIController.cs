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
        public readonly IRepository<Entry, int> _entryRepository;
        public readonly IAppHelper _appHelper;
        private readonly IQueryService _queryService;

        public ExpoAPIController(IQueryService queryService, IAppHelper appHelper, IRepository<ExpoGame, long> expoGameRepository, IRepository<ExpoTag, long> expoTagRepository, IRepository<Entry, int> entryRepository)
        {
            _appHelper = appHelper;
            _queryService = queryService;
            _expoGameRepository = expoGameRepository;
            _expoTagRepository = expoTagRepository;
            _entryRepository = entryRepository;
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
                     }).ToList()
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
    }
}
