using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/storeinfo/[action]")]
    public class StoreInfoAPIController : ControllerBase
    {

        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<StoreInfo, long> _storeInfoRepository;
        private readonly IAppHelper _appHelper;
        private readonly IHttpService _httpService;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IQueryService _queryService;

        public StoreInfoAPIController(IRepository<StoreInfo, long> storeInfoRepository, IRepository<Tag, int> tagRepository, IQueryService queryService,
        IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<ApplicationUser, string> userRepository, IHttpService httpService)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _storeInfoRepository = storeInfoRepository;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
            _httpService = httpService;
            _queryService = queryService;
        }

        /// <summary>
        /// 获取所有游戏的商店信息的
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<StoreInfoCardModel>>> GetAllGameStoreInfo()
        {
            var games = await _storeInfoRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => s.State == StoreState.OnSale && s.PriceNow != null && s.Entry != null && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false).ToListAsync();

            var model = new List<StoreInfoCardModel>();
            foreach (var item in games)
            {
                model.Add(new StoreInfoCardModel
                {
                    EvaluationCount = item.EvaluationCount,
                    BriefIntroduction = item.Entry.BriefIntroduction,
                    CutLowest = item.CutLowest,
                    CutNow = item.CutNow,
                    MainImage = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    Name = item.Entry.DisplayName,
                    OriginalPrice = item.OriginalPrice,
                    PriceLowest = item.PriceLowest,
                    PriceNow = item.PriceNow,
                    PublishTime = item.Entry.PubulishTime,
                    RecommendationRate = item.RecommendationRate,
                    PlayTime = item.PlayTime,
                    State = item.State,
                    CurrencyCode = item.CurrencyCode,
                    EstimationOwnersMax = item.EstimationOwnersMax,
                    EstimationOwnersMin = item.EstimationOwnersMin,
                    Link = item.Link,
                    PlatformName = item.PlatformName,
                    PlatformType = item.PlatformType,
                    UpdateTime = item.UpdateTime,
                    UpdateType = item.UpdateType,
                    Id=item.Entry.Id,
                });
            }

            return model;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<QueryResultModel<StoreInfoOverviewModel>> List(QueryParameterModel model)
        {
            var (items, total) = await _queryService.QueryAsync<StoreInfo, string>(_storeInfoRepository.GetAll().AsSingleQuery(), model,
                s => string.IsNullOrWhiteSpace(model.SearchText) || (s.Name.Contains(model.SearchText) || s.PlatformName.Contains(model.SearchText) || s.Link.Contains(model.SearchText)));

            return new QueryResultModel<StoreInfoOverviewModel>
            {
                Items = await items.Select(s => new StoreInfoOverviewModel
                {
                    State = s.State,
                    CutNow = s.CutNow,
                    EntryId = s.EntryId,
                    Link = s.Link,
                    Name = s.Name,
                    PlatformName = s.PlatformName,
                    PlatformType = s.PlatformType,
                    PriceNow = s.PriceNow,
                    UpdateTime = s.UpdateTime,
                    UpdateType = s.UpdateType,
                    Id = s.Id,
                }).ToListAsync(),
                Total = total,
                Parameter = model
            };
        }

        [HttpGet]
        public async Task<ActionResult<StoreInfoEditModel>> Edit(long id)
        {
            var item = await _storeInfoRepository.GetAll().FirstOrDefaultAsync(s => s.Id == id);

            if (item == null)
            {
                return NotFound("找不到商店信息");
            }

            var model = new StoreInfoEditModel
            {
                State = item.State,
                CutNow = item.CutNow,
                EntryId = item.EntryId,
                Link = item.Link,
                Name = item.Name,
                PlatformName = item.PlatformName,
                PlatformType = item.PlatformType,
                PriceNow = item.PriceNow,
                UpdateTime = item.UpdateTime,
                UpdateType = item.UpdateType,
                Id = item.Id,
                CutLowest = item.CutLowest,
                EstimationOwnersMax = item.EstimationOwnersMax,
                EstimationOwnersMin = item.EstimationOwnersMin,
                EvaluationCount = item.EvaluationCount,
                OriginalPrice = item.OriginalPrice,
                PlayTime = item.PlayTime,
                PriceLowest = item.PriceLowest,
                RecommendationRate = item.RecommendationRate,
                CurrencyCode = item.CurrencyCode,
            };

            return model;
        }

        [HttpPost]
        public async Task<Result> Edit(StoreInfoEditModel model)
        {
            var item = await _storeInfoRepository.GetAll().FirstOrDefaultAsync(s => s.Id == model.Id);

            if(item==null)
            {
                item = new StoreInfo();

                item = await _storeInfoRepository.InsertAsync(item);
            }


            item.State = model.State;
            item.CutNow = model.CutNow;
            item.EntryId = model.EntryId;
            item.Link = model.Link;
            item.Name = model.Name;
            item.PlatformName = model.PlatformName;
            item.PlatformType = model.PlatformType;
            item.PriceNow = model.PriceNow;
            item.UpdateType = model.UpdateType;
            item.CutLowest = model.CutLowest;
            item.EstimationOwnersMax = model.EstimationOwnersMax;
            item.EstimationOwnersMin = model.EstimationOwnersMin;
            item.EvaluationCount = model.EvaluationCount;
            item.OriginalPrice = item.OriginalPrice;
            item.PlayTime = model.PlayTime;
            item.PriceLowest = model.PriceLowest;
            item.RecommendationRate = model.RecommendationRate;
            item.UpdateTime = DateTime.Now.ToCstTime();

            await _storeInfoRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }
    }
}
