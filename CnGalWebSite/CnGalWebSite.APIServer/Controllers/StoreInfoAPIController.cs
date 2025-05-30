using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services;
using CnGalWebSite.Core.Services.Query;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Stores;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;

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
                .Where(s => s.State == StoreState.OnSale && s.PriceNow != null && s.Entry != null && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false)
                .ToListAsync();

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
                    Id = item.Entry.Id,
                });
            }

            return model.DistinctBy(s => s.Id).ToList();
        }

        /// <summary>
        /// 获取按年份分组的游戏销售额信息
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<GameRevenueInfoViewModel>> GetGameRevenueInfo([FromQuery] int year, [FromQuery] int page = 0, [FromQuery] int max = 20, [FromQuery] int order = 0)
        {
            var games = await _storeInfoRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry).ThenInclude(s => s.EntryStaffFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .Where(s => s.PriceNow != null && s.PriceNow > 0 && s.Revenue != null && s.Revenue > 0 && s.EstimationOwnersMax != null && s.EstimationOwnersMax > 0 /*&& s.EstimationOwnersMin != null && s.EstimationOwnersMin > 0*/)
                .Where(s => s.Entry != null && s.Entry.PubulishTime != null && (year == 0 || s.Entry.PubulishTime.Value.Year == year))
                .Where(s => s.State == StoreState.OnSale && s.PriceNow != null && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false)
                .ToListAsync();
            games = games.DistinctBy(s => s.Link).ToList();

            var groupGames = new List<StoreInfo>();

            foreach (var item in games.GroupBy(s => s.EntryId))
            {
                var temp = item.First();
                //将相同游戏的商店信息合并
                var revenue = item.Sum(s => s.Revenue);
                var estimationOwnersMin = item.Sum(s => s.EstimationOwnersMin);
                var estimationOwnersMax = item.Sum(s => s.EstimationOwnersMax);
                var evaluationCount = item.Sum(s => s.EvaluationCount);
                var price = item.Min(s => s.OriginalPrice);

                double? score = 0;
                foreach (var info in item)
                {
                    score += info.EvaluationCount * info.RecommendationRate;
                }
                temp.RecommendationRate = evaluationCount == 0 ? 0 : score / evaluationCount;
                temp.Revenue = revenue;
                temp.EstimationOwnersMin = estimationOwnersMin;
                temp.EstimationOwnersMax = estimationOwnersMax;
                temp.EvaluationCount = evaluationCount;
                temp.OriginalPrice = price;
                groupGames.Add(temp);
            }


            var gameList = groupGames
                .OrderByDescending(s => ((GameRevenueInfoOrderType)order == GameRevenueInfoOrderType.Revenue) ? s.Revenue : ((s.EstimationOwnersMax + s.EstimationOwnersMin) / 2 ?? 0))
                .Skip(page * max)
                .Take(max)
                .ToList();

            var model = new List<GameRevenueInfoCardModel>();
            foreach (var item in gameList)
            {
                model.Add(new GameRevenueInfoCardModel
                {
                    Index = page * max + gameList.IndexOf(item) + 1,
                    EvaluationCount = item.EvaluationCount ?? 0,
                    MainImage = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    Name = item.Entry.DisplayName,
                    Price = item.OriginalPrice ?? 0,
                    PublishTime = item.Entry.PubulishTime,
                    RecommendationRate = item.RecommendationRate ?? 0,
                    PlayTime = item.PlayTime ?? 0,
                    Id = item.Entry.Id,
                    Owner = (item.EstimationOwnersMax + item.EstimationOwnersMin) / 2 ?? 0,
                    Publisher = string.Join(" / ", item.Entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral == PositionGeneralType.Publisher || s.PositionGeneral == PositionGeneralType.ProductionGroup).Select(s => s.ToEntryNavigation?.Name ?? s.Name).Distinct()),
                    Revenue = item.Revenue ?? 0
                });
            }

            return new GameRevenueInfoViewModel
            {
                Items = model,
                TotalPages = (games.Count + max - 1) / max
            };
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

            if (item == null)
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
            item.CurrencyCode = model.CurrencyCode;
            item.UpdateTime = DateTime.Now.ToCstTime();

            await _storeInfoRepository.UpdateAsync(item);

            return new Result { Successful = true };
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<List<CnGalGenerationYearModel>> GetCnGalGeneration()
        {
            int[] excludeEntryIds = [196, 197, 198];

            // 获取商店信息中的游戏
            var games = await _storeInfoRepository.GetAll().AsNoTracking()
               .Include(s => s.Entry)
               .Where(s => s.EstimationOwnersMax != null && s.EstimationOwnersMax > 0)
               .Where(s => s.Entry != null && s.Entry.PubulishTime != null)
               .Where(s => s.State == StoreState.OnSale && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false)
               // 排除指定ID的词条
               .Where(s => !excludeEntryIds.Contains(s.Entry.Id))
               .ToListAsync();
            games = [.. games.DistinctBy(s => s.Link)];

            //将相同游戏的商店信息合并
            var groupGames = new List<StoreInfo>();
            foreach (var item in games.GroupBy(s => s.EntryId))
            {
                var temp = item.First();
                var revenue = item.Sum(s => s.Revenue);
                var estimationOwnersMin = item.Sum(s => s.EstimationOwnersMin);
                var estimationOwnersMax = item.Sum(s => s.EstimationOwnersMax);
                var evaluationCount = item.Sum(s => s.EvaluationCount);
                var price = item.Min(s => s.OriginalPrice);

                double? score = 0;
                foreach (var info in item)
                {
                    score += info.EvaluationCount * info.RecommendationRate;
                }
                temp.RecommendationRate = evaluationCount == 0 ? 0 : score / evaluationCount;
                temp.Revenue = revenue;
                temp.EstimationOwnersMin = estimationOwnersMin;
                temp.EstimationOwnersMax = estimationOwnersMax;
                temp.EvaluationCount = evaluationCount;
                temp.OriginalPrice = price;
                groupGames.Add(temp);
            }

            // 获取每年销量前12的游戏
            var result = groupGames.GroupBy(s => s.Entry.PubulishTime.Value.Year).Select(s => new CnGalGenerationYearModel
            {
                Year = s.First().Entry.PubulishTime.Value.Year,
                Games = [.. s.OrderByDescending(s=>(s.EstimationOwnersMax+s.EstimationOwnersMin)/2).Take(12).Select(s => new CnGalGenerationModel
                {
                    Name = s.Entry.DisplayName
                })]
            }).OrderBy(s => s.Year).ToList();

            // 获取所有游戏词条，用于补充缺失的年份和不足12个游戏的年份
            var entries = await _entryRepository.GetAll().AsNoTracking()
                .Where(e => e.Type == EntryType.Game && e.PubulishTime != null && e.IsHidden == false && string.IsNullOrWhiteSpace(e.DisplayName) == false)
                // 排除指定ID的词条
                .Where(e => !excludeEntryIds.Contains(e.Id))
                .OrderByDescending(e => e.ReaderCount)
                .Select(e => new
                {
                    e.Id,
                    e.DisplayName,
                    Year = e.PubulishTime.Value.Year,
                    e.ReaderCount
                })
                .ToListAsync();

            // 获取所有可能的年份范围 - 限制在今年往前20年
            int currentYear = DateTime.Now.Year;
            int oldestYear = currentYear - 21;
            if (oldestYear < 2007)
            {
                oldestYear = 2007;
            }

            // 确保年份范围不超出限制
            int minYear = Math.Min(
                entries.Any() ? Math.Max(entries.Min(e => e.Year), oldestYear) : currentYear,
                result.Any() ? Math.Max(result.Min(r => r.Year), oldestYear) : currentYear
            );
            int maxYear = Math.Max(
                entries.Any() ? Math.Min(entries.Max(e => e.Year), currentYear) : currentYear,
                result.Any() ? Math.Min(result.Max(r => r.Year), currentYear) : currentYear
            );

            // 补充缺失的年份和不足12个游戏的年份
            var finalResult = new List<CnGalGenerationYearModel>();
            for (int year = minYear; year <= maxYear; year++)
            {
                var yearModel = result.FirstOrDefault(r => r.Year == year);

                if (yearModel == null)
                {
                    // 如果该年份在商店信息中不存在，则创建新的年份模型
                    yearModel = new CnGalGenerationYearModel
                    {
                        Year = year,
                        Games = new List<CnGalGenerationModel>()
                    };
                }

                // 计算需要补充的游戏数量
                int needToAdd = 12 - yearModel.Games.Count;
                if (needToAdd > 0)
                {
                    // 获取该年份中阅读量最高的游戏，排除已经在列表中的游戏
                    var existingGameNames = yearModel.Games.Select(g => g.Name).ToHashSet();
                    var additionalGames = entries
                        .Where(e => e.Year == year && !existingGameNames.Contains(e.DisplayName))
                        .OrderByDescending(e => e.ReaderCount)
                        .Take(needToAdd)
                        .Select(e => new CnGalGenerationModel
                        {
                            Name = e.DisplayName
                        })
                        .ToList();

                    // 添加补充的游戏
                    yearModel.Games.AddRange(additionalGames);
                }

                // 只保留有游戏的年份
                if (yearModel.Games.Count > 0)
                {
                    finalResult.Add(yearModel);
                }
            }

            finalResult.RemoveAll(s => s.Year == currentYear);
            return finalResult.OrderBy(r => r.Year).ToList();
        }
    }
}
