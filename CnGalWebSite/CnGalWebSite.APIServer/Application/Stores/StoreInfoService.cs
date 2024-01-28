using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Stores;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Senparc.Weixin.MP.AdvancedAPIs.Card;
using System.Net.Http;
using System.Net.Http.Json;

namespace CnGalWebSite.APIServer.Application.Stores
{
    public class StoreInfoService : IStoreInfoService
    {
        private readonly IRepository<StoreInfo, long> _storeInfoRepository;
        private readonly ILogger<StoreInfoService> _logger;
        private readonly IHttpService _httpService;
        private readonly IConfiguration _configuration;

        public StoreInfoService(IRepository<StoreInfo, long> storeInfoRepository, ILogger<StoreInfoService> logger, IHttpService httpService, IConfiguration configuration)
        {
            _storeInfoRepository = storeInfoRepository;
            _logger = logger;
            _httpService = httpService;
            _configuration = configuration;
        }

        public async Task<StoreInfoViewModel> Get(PublishPlatformType platformType, string platformName, string link, string name, int entryId)
        {
            var storeInfo = await _storeInfoRepository.GetAll().AsNoTracking()
                .FirstOrDefaultAsync(s => s.PlatformType == platformType && s.PlatformName == platformName && s.Link == link && s.Name == name && s.EntryId == entryId);

            if (storeInfo == null)
            {
                storeInfo = await Update(platformType, platformName, link, name, entryId);
                _storeInfoRepository.Clear();
            }

            //复制数据

            return new StoreInfoViewModel
            {
                PlatformType = storeInfo.PlatformType,
                PlatformName = storeInfo.PlatformName,
                Link = storeInfo.Link,
                State = storeInfo.State,
                CurrencyCode = storeInfo.CurrencyCode,
                CutLowest = storeInfo.CutLowest,
                CutNow = storeInfo.CutNow,
                EstimationOwnersMax = storeInfo.EstimationOwnersMax,
                EstimationOwnersMin = storeInfo.EstimationOwnersMin,
                EvaluationCount = storeInfo.EvaluationCount,
                OriginalPrice = storeInfo.OriginalPrice,
                PlayTime = storeInfo.PlayTime,
                PriceLowest = storeInfo.PriceLowest,
                PriceNow = storeInfo.PriceNow,
                RecommendationRate = storeInfo.RecommendationRate,
                UpdateTime = storeInfo.UpdateTime,
                UpdateType = storeInfo.UpdateType,
                Name = storeInfo.Name,
            };
        }

        public async Task BatchUpdate(int max)
        {
            var date = DateTime.Now.ToCstTime().Date;

            var steams = await _storeInfoRepository.GetAll().AsNoTracking()
                .Where(s => s.UpdateTime.Date < date && s.State != StoreState.Takedown && s.UpdateType == StoreUpdateType.Automatic && s.Entry.PubulishTime != null && s.Entry.PubulishTime < date)
                .Where(s => s.PlatformType == PublishPlatformType.Steam)
                .OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.EntryId)
                .Take(max)
                .ToListAsync();

            foreach (var item in steams)
            {
                await Update(item);
            }
        }

        /// <summary>
        /// 更新价格信息
        /// </summary>
        public async Task<StoreInfo> Update(PublishPlatformType platformType, string platformName, string link, string name, int entryId)
        {
            if (entryId == 0)
            {
                return new StoreInfo();
            }

            var storeInfo = await _storeInfoRepository.GetAll().FirstOrDefaultAsync(s => s.PlatformType == platformType && s.PlatformName == platformName && s.Link == link && s.Name == name && s.EntryId == entryId);

            if (storeInfo == null)
            {
                storeInfo = new StoreInfo
                {
                    PlatformType = platformType,
                    PlatformName = platformName,
                    Name = name,
                    Link = link,
                    EntryId = entryId,
                    UpdateTime = DateTime.Now.ToCstTime()
                };

                await _storeInfoRepository.InsertAsync(storeInfo);
            }

            await Update(storeInfo);

            return storeInfo;
        }

        public async Task Update(StoreInfo storeInfo)
        {
            switch (storeInfo.PlatformType)
            {
                case PublishPlatformType.Steam:
                    await UpdateSteamInfo(storeInfo);
                    break;
                default:
                    return;
            }

            //最后更新时间
            storeInfo.UpdateTime = DateTime.Now.ToCstTime();

            await _storeInfoRepository.UpdateAsync(storeInfo);

            _logger.LogInformation("更新平台 - {platformType}, Id/链接 - {link}, 游戏 - {name}({id}) 的商店信息", storeInfo.PlatformType == PublishPlatformType.Other ? storeInfo.PlatformName : storeInfo.PlatformType.GetDisplayName(), storeInfo.Link, storeInfo.Name, storeInfo.EntryId);
        }

        #region Steam

        /// <summary>
        /// 更新Steam的商店信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateSteamInfo(StoreInfo storeInfo)
        {
            var data = new StoreInfo
            {
                Name = storeInfo.Name,
                Link = storeInfo.Link,
            };
            var officalApiTask = UpdateSteamInfoFromOfficialAPI(data);
            var officalHtmlTask = UpdateSteamInfoFromOfficialHtml(data);
            var isthereanydealTask = UpdateSteamInfoFromIsthereanydeal(data);
            var xiaoHeiHeTask = UpdateSteamInfoFromXiaoHeiHe(data);
            var gamalyticTask = UpdateSteamInfoFromGamalytic(data);

            await Task.WhenAll(officalApiTask, officalHtmlTask, isthereanydealTask, xiaoHeiHeTask, gamalyticTask);

            storeInfo.State = data.State;
            storeInfo.PriceLowest = data.PriceLowest;
            storeInfo.OriginalPrice = data.OriginalPrice;
            storeInfo.PriceNow = data.PriceNow;
            storeInfo.CutNow = data.CutNow;
            storeInfo.CutLowest = data.CutLowest;
            storeInfo.PlayTime = data.PlayTime;
            storeInfo.EvaluationCount = data.EvaluationCount;
            storeInfo.RecommendationRate = data.RecommendationRate;
            storeInfo.EstimationOwnersMax = data.EstimationOwnersMax;
            storeInfo.EstimationOwnersMin = data.EstimationOwnersMin;
            storeInfo.Revenue = data.Revenue;
        }

        /// <summary>
        /// 使用Steam官方API获取信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateSteamInfoFromOfficialAPI(StoreInfo storeInfo)
        {
            try
            {
                var json = await (await _httpService.GetClientAsync()).GetStringAsync("https://store.steampowered.com/api/appdetails/?appids=" + storeInfo.Link + "&cc=cn&filters=price_overview");
                var re = JObject.Parse(json);
                var data = re[storeInfo.Link].ToObject<SteamOfficialDataModel>();
                if (!data.Success)
                {
                    //storeInfo.State = StoreState.Takedown;
                    _logger.LogError("获取 {name} - {id} Steam官方API数据失败", storeInfo.Name, storeInfo.Link);
                    return;
                }
                //原价
                storeInfo.OriginalPrice ??= data.Data.Price_overview.Initial * 0.01;
                //现价
                storeInfo.PriceNow ??= data.Data.Price_overview.Final * 0.01;
                //折扣
                storeInfo.CutNow ??= data.Data.Price_overview.Discount_percent;
                //状态
                if (storeInfo.State == StoreState.None)
                {
                    storeInfo.State = StoreState.OnSale;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} Steam官方API数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        /// <summary>
        /// 使用 Steam 商店页面的数据
        /// </summary>
        /// <returns></returns>
        public async Task UpdateSteamInfoFromOfficialHtml(StoreInfo storeInfo)
        {
            try
            {
                var content = await (await _httpService.GetClientAsync()).GetStringAsync("https://store.steampowered.com/app/" + storeInfo.Link);

                var document = new HtmlDocument();
                document.LoadHtml(content);

                var node = document.GetElementbyId("userReviews");
                if(node == null)
                {
                    _logger.LogError("获取 {name} - {id} Steam商店页面数据失败", storeInfo.Name, storeInfo.Link);
                    return;
                }
                var text = node.ChildNodes.Count > 3
                    ? node.ChildNodes[3].ChildNodes[3].ChildNodes[5].InnerText
                    : node.ChildNodes[1].ChildNodes[3].ChildNodes[5].InnerText;
                var countStr = ToolHelper.MidStrEx(text, "the ", " ").Replace(",", "");
                var rateStr = ToolHelper.MidStrEx(text, " ", "% ");

                if (int.TryParse(countStr, out int evaluationCount))
                {
                    storeInfo.EvaluationCount ??= evaluationCount;

                }
                if (int.TryParse(rateStr, out int recommendationRate))
                {
                    storeInfo.RecommendationRate ??= recommendationRate;
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} Steam商店页面数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        /// <summary>
        /// 使用 isthereanydeal API获取信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateSteamInfoFromIsthereanydeal(StoreInfo storeInfo)
        {
            try
            {
                var json = await (await _httpService.GetClientAsync()).GetStringAsync("https://api.isthereanydeal.com/v01/game/overview/?key=" + _configuration["IsthereanydealAPIToken"] + "&region=cn&country=CN&shop=steam&ids=app%2F" + storeInfo.Link + "&allowed=steam");
                var re = JObject.Parse(json);
                var data = re["data"][$"app/{storeInfo.Link}"].ToObject<IsthereanydealDataModel>();
                if (data.Price != null)
                {
                    //原价
                    storeInfo.OriginalPrice ??= data.Price.Cut == 100 ? 0 : data.Price.Price / (100 - data.Price.Cut) * 100;
                    //现价
                    storeInfo.PriceNow ??= data.Price.Price;
                    //折扣
                    storeInfo.CutNow ??= data.Price.Cut;
                }
                if (data.Lowest != null)
                {
                    //历史最低
                    storeInfo.PriceLowest ??= data.Lowest.Price;
                    //历史最高折扣
                    storeInfo.CutLowest ??= data.Lowest.Cut;
                }
                //状态
                if (storeInfo.State == StoreState.None)
                {
                    if (data.Price != null && data.Lowest != null)
                    {
                        storeInfo.State = StoreState.OnSale;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} isthereanydeal 数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        /// <summary>
        /// 使用 小黑盒 API获取信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateSteamInfoFromXiaoHeiHe(StoreInfo storeInfo)
        {
            try
            {
                var data = await _httpService.GetAsync<XiaoHeiHeDataModel>("https://api.xiaoheihe.cn/game/get_game_detail/?h_src=game_rec_a&appid=" + storeInfo.Link);
                if (data.Status != "ok")
                {
                    _logger.LogError("获取 {name} - {id} 小黑盒API数据失败", storeInfo.Name, storeInfo.Link);
                    return;
                }
                if (data.Result.Is_free)
                {
                    //原价
                    storeInfo.OriginalPrice ??= 0;
                    //现价
                    storeInfo.PriceNow ??= 0;
                    //折扣
                    storeInfo.CutNow ??= 0;
                    //历史最低
                    storeInfo.PriceLowest ??= 0;
                    //历史最高折扣
                    storeInfo.CutLowest ??= 0;
                    //状态
                    if (storeInfo.State == StoreState.None)
                    {
                        storeInfo.State = StoreState.OnSale;
                    }
                }
                else if (data.Result.Price != null)
                {
                    //原价
                    storeInfo.OriginalPrice ??= double.Parse(data.Result.Price.Initial);
                    //现价
                    storeInfo.PriceNow ??= double.Parse(data.Result.Price.Current);
                    //折扣
                    storeInfo.CutNow ??= data.Result.Price.Discount;
                    //历史最低
                    storeInfo.PriceLowest ??= double.Parse(data.Result.Price.Lowest_price_raw);
                    //历史最高折扣
                    storeInfo.CutLowest ??= data.Result.Price.Lowest_discount;
                    //状态
                    if (storeInfo.State == StoreState.None)
                    {
                        storeInfo.State = StoreState.OnSale;
                    }
                }
                if (data.Result.Positive_desc?.Contains('%') ?? false)
                {
                    storeInfo.RecommendationRate ??= int.Parse(data.Result.Positive_desc.MidStrEx("：", "%"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} 小黑盒API数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        /// <summary>
        /// 使用 Gamalytic API获取信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateSteamInfoFromGamalytic(StoreInfo storeInfo)
        {
            try
            {
                var data = await _httpService.GetAsync<GamalyticDataModel>("https://api.gamalytic.com/game/" + storeInfo.Link);

                //评测数
                storeInfo.EvaluationCount ??= data.ReviewsSteam;
                //好评率
                storeInfo.RecommendationRate ??= data.ReviewScore;
                //平均游玩时长
                storeInfo.PlayTime ??= (int)(data.AvgPlaytime * 60);
                //估计拥有人数上限
                storeInfo.EstimationOwnersMax ??= (int)(data.Owners * (2 - data.Accuracy));
                //估计拥有人数下限
                storeInfo.EstimationOwnersMin ??= (int)(data.Owners * (data.Accuracy));
                //销售额
                storeInfo.Revenue ??= (int)(data.Revenue * 7);

                //状态
                if (storeInfo.State == StoreState.None && data.Unreleased)
                {
                    storeInfo.State = StoreState.NotPublished;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} gamalytic 数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        #endregion
    }
}
