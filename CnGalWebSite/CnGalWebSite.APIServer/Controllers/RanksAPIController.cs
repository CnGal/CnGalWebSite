using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.DataReositories;
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

namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/ranks/[action]")]
    public class RanksAPIController : ControllerBase
    {
        private readonly IRepository<Rank, long> _rankRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<RankUser, long> _rankUserRepository;
        private readonly IRankService _rankService;

        public RanksAPIController(IRepository<Rank, long> rankRepository, IRepository<RankUser, long> rankUserRepository, IRankService rankService,
            IRepository<ApplicationUser, string> userRepository)
        {
            _rankRepository = rankRepository;
            _rankUserRepository = rankUserRepository;
            _rankService = rankService;
            _userRepository = userRepository;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> CreateRankAsync(CreateRankModel model)
        {
            //查看名称是否冲突
            if (await _rankRepository.GetAll().AnyAsync(s => s.Name == model.Name))
            {
                return new Result { Successful = false, Error = "已经存在该名称的头衔" };
            }

            var rank = new Rank
            {
                Styles = model.Styles,
                CSS = model.CSS,
                CreateTime = DateTime.Now.ToCstTime(),
                LastEditTime = DateTime.Now.ToCstTime(),
                Name = model.Name,
                Text = model.Text,
                Type = model.Type,
                Image = model.Image
            };
            await _rankRepository.InsertAsync(rank);

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenRankAsync(HiddenRankModel model)
        {

            await _rankRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<EditRankViewModel>> EditRankAsync(long id)
        {
            var rank = await _rankRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (rank == null)
            {
                return NotFound("无法找到该头衔");
            }

            var model = new EditRankViewModel
            {
                CSS = rank.CSS,
                Styles = rank.Styles,
                Name = rank.Name,
                Text = rank.Text,
                Id = rank.Id,
                Type=rank.Type,
                Image=rank.Image
            };

            return model;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditRankAsync(EditRankViewModel model)
        {
            var rank = await _rankRepository.FirstOrDefaultAsync(s => s.Id == model.Id);
            if (rank == null)
            {
                return NotFound("无法找到该头衔");
            }
            if (rank.Name != model.Name)
            {
                if (await _rankRepository.GetAll().AnyAsync(s => s.Name == model.Name))
                {
                    return new Result { Successful = false, Error = "已经存在该名称的头衔" };
                }
            }
            rank.Name = model.Name;
            rank.CSS = model.CSS;
            rank.Styles = model.Styles;
            rank.Text = model.Text;
            rank.Type = model.Type;
            rank.Image = model.Image;

            await _rankRepository.UpdateAsync(rank);

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> RemoveUserRankAsync(RemoveUserRankModel model)
        {
            await _rankUserRepository.DeleteRangeAsync(s => model.RankIds.Contains(s.RankId) && model.UserIds.Contains(s.ApplicationUserId));

            return new Result { Successful = true };
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListRankUserAloneModel>>> GetRankUserListAsync(RankUsersPagesInfor input)
        {
            var dtos = await _rankService.GetPaginatedResult(input.Options, input.SearchModel, input.RankId);

            return dtos;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<BootstrapBlazor.Components.QueryData<ListUserRankAloneModel>>> GetUserRankListAsync(UserRanksPagesInfor input)
        {
            var dtos = await _rankService.GetPaginatedResult(input.Options, input.SearchModel, input.UserId);

            return dtos;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> EditRankPriorityAsync(EditRankPriorityViewModel model)
        {
            await _rankRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.Priority, b => b.Priority + model.PlusPriority).ExecuteAsync();

            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult<Result>> HiddenUserRankAsync(HiddenUserRankModel model)
        {
            await _rankUserRepository.GetRangeUpdateTable().Where(s => model.Ids.Contains(s.Id)).Set(s => s.IsHidden, b => model.IsHidden).ExecuteAsync();
            return new Result { Successful = true };
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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
    }
}
