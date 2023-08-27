using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ranks/[action]")]
    public class RanksAPIController : ControllerBase
    {
        private readonly IRepository<Rank, long> _rankRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<RankUser, long> _rankUserRepository;
        private readonly IRankService _rankService;
        private readonly IQueryService _queryService;

        public RanksAPIController(IRepository<Rank, long> rankRepository, IRepository<RankUser, long> rankUserRepository, IRankService rankService, IQueryService queryService,
            IRepository<ApplicationUser, string> userRepository)
        {
            _rankRepository = rankRepository;
            _rankUserRepository = rankUserRepository;
            _rankService = rankService;
            _userRepository = userRepository;
            _queryService = queryService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenRankAsync(HiddenRankModel model)
        {

            await _rankRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<RankEditModel>> EditAsync(long id)
        {
            var rank = await _rankRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (rank == null)
            {
                return NotFound("无法找到该头衔");
            }

            var model = new RankEditModel
            {
                CSS = rank.CSS,
                Styles = rank.Styles,
                Name = rank.Name,
                Text = rank.Text,
                Id = rank.Id,
                Type=rank.Type,
                Image=rank.Image,
                Priority=rank.Priority,
            };

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditAsync(RankEditModel model)
        {
            Rank item = null;
            if (model.Id == 0)
            {
                item = await _rankRepository.InsertAsync(new Rank
                {
                    CSS = model.CSS,
                    Styles = model.Styles,
                    Name = model.Name,
                    Text = model.Text,
                    Id = model.Id,
                    Type = model.Type,
                    Image = model.Image,
                    Priority = model.Priority,
                    CreateTime=DateTime.Now.ToCstTime()
                });
                model.Id = item.Id;
                _rankRepository.Clear();
            }

            item = await _rankRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Successful = false, Error = "项目不存在" };
            }

            item.Name = model.Name;
            item.CSS = model.CSS;
            item.Styles = model.Styles;
            item.Text = model.Text;
            item.Type = model.Type;
            item.Image = model.Image;
            item.Priority = model.Priority;

            item.LastEditTime = DateTime.Now.ToCstTime();

            await _rankRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> RemoveUserRankAsync(RemoveUserRankModel model)
        {
            await _rankUserRepository.GetAll().Where(s => model.RankIds.Contains(s.RankId) && model.UserIds.Contains(s.ApplicationUserId)).ExecuteDeleteAsync();

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRankUserAloneModel>>> GetRankUserListAsync(RankUsersPagesInfor input)
        {
            var dtos = await _rankService.GetPaginatedResult(input.Options, input.SearchModel, input.RankId);

            return dtos;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListUserRankAloneModel>>> GetUserRankListAsync(UserRanksPagesInfor input)
        {
            var dtos = await _rankService.GetPaginatedResult(input.Options, input.SearchModel, input.UserId);

            return dtos;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> EditRankPriorityAsync(EditRankPriorityViewModel model)
        {
            await _rankRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.Priority, b => b.Priority + model.PlusPriority));

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenUserRankAsync(HiddenUserRankModel model)
        {
            await _rankUserRepository.GetAll().Where(s => model.Ids.Contains(s.Id)).ExecuteUpdateAsync(s=>s.SetProperty(s => s.IsHidden, b => model.IsHidden));
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result>> AddUserRankAsync(AddUserRankModel model)
        {
            //检查是否存在
            if (await _rankRepository.GetAll().AnyAsync(s => s.Id == model.RankId) == false)
            {
                return new Result { Successful = false, Error = "未找到该头衔" };
            }
            if (await _userRepository.GetAll().AnyAsync(s => s.Id == model.UserId) == false)
            {
                return new Result { Successful = false, Error = "未找到该用户" };
            }

            await _rankUserRepository.InsertAsync(new RankUser
            {
                ApplicationUserId = model.UserId,
                RankId = model.RankId,
                IsHidden = false,
                Time = DateTime.Now.ToCstTime(),
            });

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<RankOverviewModel>> ListRanks(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Rank, long>(_rankRepository.GetAll().AsSingleQuery().Include(s=>s.RankUsers), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Styles.Contains(model.SearchText) || s.CSS.Contains(model.SearchText) || s.Name.Contains(model.SearchText) || s.Text.Contains(model.SearchText)));

            return new QueryResultModel<RankOverviewModel>
            {
                Items = await items.Select(s => new RankOverviewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsHidden = s.IsHidden,
                    CreateTime = s.CreateTime,
                    Styles = s.Styles,
                    CSS = s.CSS,
                    LastEditTime = s.LastEditTime,
                    Text = s.Text,
                    Priority = s.Priority,
                    Count = s.RankUsers.Count,
                    Type = s.Type,
                    Image = s.Image,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }
    }
}
