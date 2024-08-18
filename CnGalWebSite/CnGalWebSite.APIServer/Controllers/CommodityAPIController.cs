using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.OperationRecords;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using ReverseMarkdown.Converters;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;
using System;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{
    [Route("api/commodities/[action]")]
    [Authorize]
    [ApiController]
    public class CommodityAPIController : ControllerBase
    {
        private readonly IRepository<Commodity, long> _commodityRepository;
        private readonly IRepository<CommodityCode, long> _commodityCodeRepository;
        private readonly IRepository<ApplicationUserCommodity, long> _commodityUserRepository;
        private readonly IQueryService _queryService;
        private readonly IAppHelper _appHelper;
        private readonly IRepository<ApplicationUser, long> _userRepository;
        private readonly IRepository<UserIntegral, string> _userIntegralRepository;
        private readonly IUserService _userService;
        private readonly IOperationRecordService _operationRecordService;
        private readonly ILogger<CommodityAPIController> _logger;

        public CommodityAPIController(IRepository<Commodity, long> commodityRepository, IQueryService queryService, IAppHelper appHelper, IRepository<ApplicationUser, long> userRepository, IRepository<ApplicationUserCommodity, long> commodityUserRepository,
            IRepository<UserIntegral, string> userIntegralRepository, IUserService userService, IRepository<CommodityCode, long> commodityCodeRepository, IOperationRecordService operationRecordService, ILogger<CommodityAPIController> logger)
        {
            _commodityRepository = commodityRepository;
            _queryService = queryService;
            _appHelper = appHelper;
            _userRepository = userRepository;
            _commodityUserRepository = commodityUserRepository;
            _userIntegralRepository = userIntegralRepository;
            _userService = userService;
            _commodityCodeRepository = commodityCodeRepository;
            _operationRecordService = operationRecordService;
            _logger = logger;
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
                    IsHidden = model.IsHidden,
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
            var commodities = await _commodityRepository.GetAll().Where(s => s.IsHidden == false).OrderByDescending(s => s.Priority).Select(s => new CommodityUserModel
            {
                BriefIntroduction = s.BriefIntroduction,
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Type = s.Type,
                Value = s.Value,
                Image = s.Image,
                IsHidden = s.IsHidden,
                Priority = s.Priority,
            }).ToListAsync();

            foreach (var item in commodities)
            {
                item.IsOwned = user.Commodities.Any(s => s.Id == item.Id);
            }

            return commodities;
        }

        [HttpPost]
        public async Task<Result> BuyCommodity(BuyCommodityModel model)
        {
            var commodity = await _commodityRepository.GetAll().AsNoTracking().FirstOrDefaultAsync(s => s.Id == model.Id);

            if (commodity == null)
            {
                return new Result { Successful = false, Error = "目标商品不存在" };
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            if (user.GCoins < commodity.Price)
            {
                return new Result { Successful = false, Error = "没有足够的G币" };
            }

            if (await _commodityUserRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.CommodityId == commodity.Id))
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


        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<CommodityCodeEditModel>> EditCodeAsync(long id)
        {
            var item = await _commodityCodeRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null)
            {
                return NotFound("无法找到该目标");
            }

            var model = new CommodityCodeEditModel
            {
                Id = item.Id,
                Code = item.Code,
                Count = item.Count,
                Type = item.Type,
                CanRedeemed = item.CanRedeemed,
                Hide = item.Hide,
            };

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Result>> EditCodeAsync(CommodityCodeEditModel model)
        {
            CommodityCode item = null;
            if (model.Id == 0)
            {
                item = await _commodityCodeRepository.InsertAsync(new CommodityCode
                {
                    CreateTime = DateTime.Now.ToCstTime(),
                    CanRedeemed = model.CanRedeemed,
                    Code = model.Code,
                    Count = model.Count,
                    Type = model.Type,
                    Hide = model.Hide,
                });
                model.Id = item.Id;
                _commodityCodeRepository.Clear();
            }

            item = await _commodityCodeRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);


            if (item == null)
            {
                return new Result { Successful = false, Error = "目标不存在" };
            }


            // 如果兑换码已经被兑换 修改为不可兑换需要退还G币
            if (item.CanRedeemed != model.CanRedeemed && item.Redeemed && item.ApplicationUserId != null && item.Type == CommodityCodeType.GCoins)
            {
                if (model.CanRedeemed == false)
                {
                    await _userService.TryAddGCoins(item.ApplicationUserId, UserIntegralSourceType.CommodityCode, -item.Count, $"{item.Code} 修改为不可兑换，退回G币：{item.Count}");
                }
                else
                {
                    await _userService.TryAddGCoins(item.ApplicationUserId, UserIntegralSourceType.CommodityCode, item.Count, $"{item.Code} 修改为可兑换，增加G币：{item.Count}");
                }
            }


            item.CanRedeemed = model.CanRedeemed;
            item.Code = model.Code;
            item.Type = model.Type;
            item.Count = model.Count;
            item.Hide = model.Hide;

            item.UpdateTime = DateTime.Now.ToCstTime();

            await _commodityCodeRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<CommodityCodeOverviewModel>> ListCode(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<CommodityCode, long>(_commodityCodeRepository.GetAll().AsSingleQuery().Include(s => s.ApplicationUser), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Code.Contains(model.SearchText)));

            return new QueryResultModel<CommodityCodeOverviewModel>
            {
                Items = await items.Select(s => new CommodityCodeOverviewModel
                {
                    Id = s.Id,
                    CanRedeemed = s.CanRedeemed,
                    Code = s.Code,
                    Count = s.Count,
                    CreateTime = s.CreateTime,
                    UpdateTime = s.UpdateTime,
                    Hide = s.Hide,
                    Redeemed = s.Redeemed,
                    RedeemedTime = s.RedeemedTime,
                    Type = s.Type,
                    UserId = s.ApplicationUser == null ? "" : s.ApplicationUser.Id,
                    UserName = s.ApplicationUser == null ? "" : s.ApplicationUser.UserName,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }


        [HttpPost]
        public async Task<ActionResult<Result>> RedeemedCommodityCode(RedeemedCommodityCodeModel model)
        {
            //提前判断是否通过人机验证
            if (_appHelper.CheckRecaptcha(model.Verification) == false)
            {
                return BadRequest(new Result { Error = "没有通过人机验证" });
            }

            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);
            if (user == null)
            {
                return new Result { Error = "未找到该用户" };
            }


            var code = await _commodityCodeRepository.GetAll().FirstOrDefaultAsync(s => s.Code == model.Code);
            if (code == null)
            {
                return new Result { Successful = false, Error = "没有查找到这个订单，数据更新会有延迟，可以稍后再来查询（也可以在B站私信询问）" };
            }

            if (code.Redeemed)
            {
                return new Result { Successful = false, Error = "这个订单已经兑换过了（有疑问可以在B站私信询问）" };
            }

            if (code.CanRedeemed == false)
            {
                return new Result { Successful = false, Error = "确认收货后才能兑换，状态可能不会及时更新，请耐心等待（也可以在B站私信询问）" };
            }



            var msg = "";
            if (code.Type == CommodityCodeType.GCoins)
            {
                await _userService.TryAddGCoins(user.Id, UserIntegralSourceType.CommodityCode, code.Count, $"兑换码：{code.Code}");
                msg = $"成功兑换 {code.Count} G币";
            }
            else
            {
                return new Result { Successful = false, Error = "未知的兑换码类型，请联系管理员" };
            }

            code.ApplicationUserId = user.Id;
            code.Redeemed = true;
            code.RedeemedTime = DateTime.Now.ToCstTime();
            await _commodityCodeRepository.UpdateAsync(code);


            try
            {
                await _operationRecordService.AddOperationRecord(OperationRecordType.Redeemed, code.Code, user, model.Identification, HttpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户 {Name}({Id})身份识别失败", user.UserName, user.Id);
            }


            return new Result { Successful = true, Error = msg };
        }
    }
}
