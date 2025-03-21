using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Stores;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

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
                .Where(s => s.PlatformType == PublishPlatformType.Steam || s.PlatformType == PublishPlatformType.TapTap)
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
                case PublishPlatformType.TapTap:
                    await UpdateTapTapInfo(storeInfo);
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


            await UpdateSteamInfoFromOfficialAPI(data);
            await UpdateSteamInfoFromOfficialHtml(data);
            await UpdateSteamInfoFromIsthereanydeal(data);
            await UpdateSteamInfoFromXiaoHeiHe(data);
            await UpdateSteamInfoFromGamalytic(data);
            await UpdateSteamInfoFromVginsights(data);


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
                if (storeInfo.State == StoreState.None && storeInfo.PriceNow > 0)
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
                if (node == null)
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
                var id = await _httpService.GetAsync<IsthereanydealGetIdModel>($"https://api.isthereanydeal.com/games/lookup/v1?key={_configuration["IsthereanydealAPIToken"]}&appid={storeInfo.Link}");

                if (id.found == false)
                {
                    return;
                }

                var data = await _httpService.PostAsync<List<string>, IsthereanydealDataModel>($"https://api.isthereanydeal.com/games/overview/v2?key={_configuration["IsthereanydealAPIToken"]}&shops=61&country=CN", [id.game.id]);

                if (data.prices == null || data.prices.Count == 0)
                {
                    return;
                }
                var price = data.prices[0];

                if (price.current != null)
                {
                    //原价
                    storeInfo.OriginalPrice ??= price.current.regular.amount;
                    //现价
                    storeInfo.PriceNow ??= price.current.price.amount;
                    //折扣
                    storeInfo.CutNow ??= price.current.cut;
                }
                if (price.lowest != null)
                {
                    //历史最低
                    storeInfo.PriceLowest ??= price.lowest.price.amount;
                    //历史最高折扣
                    storeInfo.CutLowest ??= price.lowest.cut;
                }
                //状态
                if (storeInfo.State == StoreState.None)
                {
                    if (price.current != null && price.lowest != null)
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
                var data = await _httpService.GetAsync<XiaoHeiHeDataModel>(_configuration["HeyboxGetGameDetailUrl"] + storeInfo.Link);
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
                    storeInfo.OriginalPrice ??= data.Result.Price.Initial;
                    //现价
                    storeInfo.PriceNow ??= data.Result.Price.Current;
                    //折扣
                    storeInfo.CutNow ??= data.Result.Price.Discount;
                    //历史最低
                    storeInfo.PriceLowest ??= data.Result.Price.Lowest_price_raw;
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
                //估计拥有人数上限 - 取平均
                if (storeInfo.EstimationOwnersMax.HasValue)
                    storeInfo.EstimationOwnersMax = (storeInfo.EstimationOwnersMax.Value + (int)data.Owners) / 2;
                else
                    storeInfo.EstimationOwnersMax = (int)data.Owners;

                //估计拥有人数下限 - 取平均
                if (storeInfo.EstimationOwnersMin.HasValue)
                    storeInfo.EstimationOwnersMin = (storeInfo.EstimationOwnersMin.Value + (int)data.Owners) / 2;
                else
                    storeInfo.EstimationOwnersMin = (int)data.Owners;

                //销售额 - 取平均
                if (storeInfo.Revenue.HasValue)
                    storeInfo.Revenue = (storeInfo.Revenue.Value + (int)(data.Revenue * 7)) / 2;
                else
                    storeInfo.Revenue = (int)(data.Revenue * 7);

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


        /// <summary>
        /// 使用 Vginsights API获取信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateSteamInfoFromVginsights(StoreInfo storeInfo)
        {
            try
            {
                var data = await _httpService.GetAsync<VginsightsDataModel>("https://vginsights.com/api/v1/game/" + storeInfo.Link);

                //评测数
                storeInfo.EvaluationCount ??= data.reviews;
                //好评率
                storeInfo.RecommendationRate ??= data.rating;
                //估计拥有人数上限 - 取平均
                if (storeInfo.EstimationOwnersMax.HasValue)
                    storeInfo.EstimationOwnersMax = (storeInfo.EstimationOwnersMax.Value + (int)data.units_sold_vgi) / 2;
                else
                    storeInfo.EstimationOwnersMax = (int)data.units_sold_vgi;

                //估计拥有人数下限 - 取平均
                if (storeInfo.EstimationOwnersMin.HasValue)
                    storeInfo.EstimationOwnersMin = (storeInfo.EstimationOwnersMin.Value + (int)data.units_sold_vgi) / 2;
                else
                    storeInfo.EstimationOwnersMin = (int)data.units_sold_vgi;

                if (int.TryParse(data.revenue_vgi, out var result))
                {
                    //销售额 - 取平均
                    if (storeInfo.Revenue.HasValue)
                        storeInfo.Revenue = (storeInfo.Revenue.Value + (int)(result * 7)) / 2;
                    else
                        storeInfo.Revenue = (int)(result * 7);
                }


                //状态
                if (storeInfo.State == StoreState.None)
                {
                    storeInfo.State = data.isReleased ? StoreState.OnSale : StoreState.NotPublished;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} Vginsights 数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        #endregion

        #region TapTap

        /// <summary>
        /// 更新TapTap的商店信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateTapTapInfo(StoreInfo storeInfo)
        {
            var data = new StoreInfo
            {
                Name = storeInfo.Name,
                Link = storeInfo.Link,
            };
            var officalApiTask = UpdateTapTapInfoFromOfficialAPI(data);

            await Task.WhenAll(officalApiTask);

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
        /// 使用TapTap官方API获取信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateTapTapInfoFromOfficialAPI(StoreInfo storeInfo)
        {
            try
            {
                var data = await _httpService.GetAsync<TapTapDataModel>("https://www.taptap.cn/webapiv2/app/v4/detail?X-UA=V%3D1%26PN%3DWebApp%26LANG%3Dzh_CN%26VN_CODE%3D102%26LOC%3DCN%26PLT%3DPC%26DS%3DAndroid%26UID%3D36729d9f-0ebc-4d8e-9959-7866f4a1c75f%26OS%3DWindows%26OSV%3D10%26DT%3DPC&id=" + storeInfo.Link);

                if (data.Redirect != null)
                {
                    data = await _httpService.GetAsync<TapTapDataModel>("https://www.taptap.cn/webapiv2/app/v4/detail?X-UA=V%3D1%26PN%3DWebApp%26LANG%3Dzh_CN%26VN_CODE%3D102%26LOC%3DCN%26PLT%3DPC%26DS%3DAndroid%26UID%3D36729d9f-0ebc-4d8e-9959-7866f4a1c75f%26OS%3DWindows%26OSV%3D10%26DT%3DPC&id=" + data.Redirect.web_url.Split('/').LastOrDefault());
                }

                if (data.Data.Code == -1 || (data.Data.Price == null && data.Data.Stat == null))
                {
                    //storeInfo.State = StoreState.Takedown;
                    _logger.LogError("获取 {name} - {id} TapTap官方API数据失败", storeInfo.Name, storeInfo.Link);
                    return;
                }
                // 免费游戏
                if (data.Data.Price == null && data.Data.Stat != null && data.Data.Stat.hits_total != 0)
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
                else if (data.Data.Price != null)
                {
                    //原价
                    if (storeInfo.OriginalPrice == null && string.IsNullOrWhiteSpace(data.Data.Price?.taptap_original) == false)
                    {
                        if (int.TryParse(data.Data.Price.taptap_original.Replace("￥", ""), out int price))
                        {
                            storeInfo.OriginalPrice = price;
                        }
                    }

                    //现价
                    if (storeInfo.PriceNow == null && string.IsNullOrWhiteSpace(data.Data.Price?.taptap_current) == false)
                    {
                        if (int.TryParse(data.Data.Price.taptap_current.Replace("￥", ""), out int price))
                        {
                            storeInfo.PriceNow = price;
                        }
                    }
                    //折扣
                    storeInfo.CutNow ??= data.Data.Price?.discount_rate;
                    //游玩人数
                    storeInfo.EstimationOwnersMax ??= data.Data.Stat.bought_count;
                    storeInfo.EstimationOwnersMin ??= data.Data.Stat.bought_count;
                    storeInfo.Revenue ??= (int)(data.Data.Stat.bought_count * storeInfo.OriginalPrice.Value);
                    //状态
                    if (storeInfo.State == StoreState.None)
                    {
                        storeInfo.State = StoreState.OnSale;
                    }
                }

                // 评测
                if (storeInfo.RecommendationRate == null && string.IsNullOrWhiteSpace(data.Data.Stat?.Rating?.Score) == false)
                {
                    if (double.TryParse(data.Data.Stat?.Rating?.Score, out double score))
                    {
                        storeInfo.RecommendationRate ??= score / (double)data.Data.Stat?.Rating?.Max * 100;
                    }
                }
                storeInfo.EvaluationCount ??= data.Data.Stat?.review_count;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取 {name} - {id} TapTap官方API数据失败", storeInfo.Name, storeInfo.Link);
            }
        }

        #endregion
    }
}
