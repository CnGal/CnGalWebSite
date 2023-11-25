using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Route("api/commodities/[action]")]
    [Authorize]
    [ApiController]
    public class CommodityAPIController : ControllerBase
    {
        private readonly IRepository<Commodity, long> _commodityRepository;
        private readonly IRepository<ApplicationUserCommodity, long> _commodityUserRepository;
        private readonly IQueryService _queryService;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<ApplicationUser, long> _userRepository;
        private readonly IRepository<UserIntegral, string> _userIntegralRepository;
        private readonly IUserService _userService;

        public CommodityAPIController(IRepository<Commodity, long> commodityRepository, IQueryService queryService, IAppHelper appHelper, IRepository<ApplicationUser, long> userRepository, IRepository<ApplicationUserCommodity, long> commodityUserRepository,
            IRepository<UserIntegral, string> userIntegralRepository, IUserService userService)
        {
            _commodityRepository= commodityRepository;
            _queryService= queryService;
            _appHelper= appHelper;
            _userRepository= userRepository;
            _commodityUserRepository= commodityUserRepository;
            _userIntegralRepository= userIntegralRepository;
            _userService= userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<CommodityEditModel>> EditAsync(long id)
        {
            var item = await _commodityRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new CommodityEditModel
            {
                BriefIntroduction = item.BriefIntroduction,
                Id = item.Id,
                Name = item.Name,
                Price = item.Price,
                Type = item.Type,
                Value = item.Value,
                Priority = item.Priority,
                IsHidden = item.IsHidden,
                Image = item.Image,
            };

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditAsync(CommodityEditModel model)
        {
            Commodity item = null;
            if (model.Id == 0)
            {
                item = await _commodityRepository.InsertAsync(new Commodity
                {
                    BriefIntroduction = model.BriefIntroduction,
                    Name = model.Name,
                    Price = model.Price,
                    Type = model.Type,
                    Value = model.Value,
                    IsHidden=model.IsHidden,
                    Priority = model.Priority,
                    Image = model.Image,
                    CreateTime = DateTime.Now.ToCstTime()
                });
                model.Id = item.Id;
                _commodityRepository.Clear();
            }

            item = await _commodityRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Successful = false, Error = "项目不存在" };
            }

            item.BriefIntroduction = model.BriefIntroduction;
            item.Name = model.Name;
            item.Price = model.Price;
            item.Type = model.Type;
            item.IsHidden = model.IsHidden;
            item.Priority = model.Priority;
            item.Value = model.Value;
            item.Image = model.Image;

            item.LastEditTime = DateTime.Now.ToCstTime();

            await _commodityRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<CommodityOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<Commodity, long>(_commodityRepository.GetAll().AsSingleQuery().Include(s => s.Users), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.BriefIntroduction.Contains(model.SearchText) || s.Value.Contains(model.SearchText)));

            return new QueryResultModel<CommodityOverviewModel>
            {
                Items = await items.Select(s => new CommodityOverviewModel
                {
                    BriefIntroduction = s.BriefIntroduction,
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    Type = s.Type,
                    Value = s.Value,
                    UserCount = s.Users.Count,
                    IsHidden = s.IsHidden,
                    Priority = s.Priority,
                    Image = s.Image,
                    LastEditTime = s.LastEditTime,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<List<CommodityUserModel>> GetAllCommodities()
        {
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            user = await _userRepository.GetAll().Include(s => s.Commodities).FirstOrDefaultAsync(s => s.Id == user.Id);
            var commodities=await _commodityRepository.GetAll().Where(s=>s.IsHidden==false).OrderByDescending(s=>s.Priority).Select(s=>new CommodityUserModel
            {
                BriefIntroduction=s.BriefIntroduction,
                Id=s.Id,
                Name = s.Name,
                Price = s.Price,
                Type = s.Type,
                Value = s.Value,
                Image = s.Image,
                IsHidden=s.IsHidden,
                Priority=s.Priority,
            }).ToListAsync();

            foreach(var item in commodities)
            {
                item.IsOwned=user.Commodities.Any(s=>s.Id==item.Id);
            }

            return commodities;
        }

        [HttpPost]
        public async Task<Result> BuyCommodity(BuyCommodityModel model)
        {
            var commodity = await _commodityRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);

            if(commodity == null)
            {
                return new Result { Successful = false,Error="目标商品不存在" };
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if(user.GCoins<commodity.Price)
            {
                return new Result { Successful = false, Error = "没有足够的G币" };
            }

            if(await _commodityUserRepository.AnyAsync(s=>s.ApplicationUserId==user.Id&&s.CommodityId == commodity.Id))
            {
                return new Result { Successful = false, Error = "已经拥有此商品" };
            }

            await _userService.TryAddGCoins(user.Id, UserIntegralSourceType.BuyCommodity, -commodity.Price, $"给看板娘买{commodity.Name}");
            await _commodityUserRepository.InsertAsync(new ApplicationUserCommodity
            {
                ApplicationUserId = user.Id,
                CommodityId = commodity.Id,
            });

            return new Result { Successful = true };
        }
    }
}
