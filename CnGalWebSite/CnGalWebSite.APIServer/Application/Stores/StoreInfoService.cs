using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Net.Http;

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
                storeInfo = await Update(platformType, platformName, link,name, entryId);
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

            var storeInfo = await _storeInfoRepository.GetAll().FirstOrDefaultAsync(s => s.PlatformType == platformType && s.PlatformName == platformName && s.Link == link && s.Name == name&&s.EntryId==entryId);

            if (storeInfo == null)
            {
                storeInfo = new StoreInfo
                {
                    PlatformType = platformType,
                    PlatformName = platformName,
                    Name= name,
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
                    await UpdateFromSteam(storeInfo);
                    break;
                default:
                    return;
            }

            //最后更新时间
            storeInfo.UpdateTime = DateTime.Now.ToCstTime();

            await _storeInfoRepository.UpdateAsync(storeInfo);

            _logger.LogInformation("更新平台 - {platformType}, Id/链接 - {link}, 词条 - {id} 的商店信息", storeInfo.PlatformType == PublishPlatformType.Other ? storeInfo.PlatformName : storeInfo.PlatformType.GetDisplayName(),storeInfo.Link,storeInfo.EntryId);
        }

        #region Steam

        /// <summary>
        /// 更新Steam信息
        /// </summary>
        /// <param name="storeInfo"></param>
        /// <returns></returns>
        public async Task UpdateFromSteam(StoreInfo storeInfo)
        {
            //判断是否下架
            if (storeInfo.State == StoreState.Takedown)
            {
                return;
            }

            //判断是否为免费游戏
            //if (storeInfo.OriginalPrice != 0)
            {
                await UpdateSteamInforByRemoteAPI(storeInfo);
            }

            //获取附加信息
            if (storeInfo.State== StoreState.OnSale)
            {
                await GetSteamAdditionInformationAsync(storeInfo);
            }
        }

        /// <summary>
        /// 获取Steam附加信息
        /// </summary>
        /// <param name="steam"></param>
        /// <returns></returns>
        public async Task GetSteamAdditionInformationAsync(StoreInfo steam)
        {
            try
            {
                var content = await (await _httpService.GetClientAsync()).GetStringAsync(_configuration["SteamspyUrl"] + "api.php?request=appdetails&appid=" + steam.Link);
                var json = JObject.Parse(content);

                steam.EvaluationCount = json["positive"].ToObject<int>() + json["negative"].ToObject<int>();
                if (steam.EvaluationCount != 0)
                {
                    steam.RecommendationRate = json["positive"].ToObject<int>() * 100.0 / steam.EvaluationCount;
                }


                var minutes = json["average_forever"].ToObject<int>();
                if (minutes > 3000)
                {
                    steam.PlayTime = 0;
                }
                else
                {
                    steam.PlayTime = minutes;
                }


                var times = json["owners"].ToObject<string>().Split("..");

                steam.EstimationOwnersMin = int.Parse(times[0].Replace(" ", "").Replace(",", ""));
                steam.EstimationOwnersMax = int.Parse(times[1].Replace(" ", "").Replace(",", ""));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Id：{id} 获取Steam附加信息失败", steam.Link);
            }

        }

        /// <summary>
        /// 获取Steam价格
        /// </summary>
        /// <param name="steam"></param>
        /// <returns></returns>
        private async Task UpdateSteamInforByRemoteAPI(StoreInfo steam)
        {
            //获取信息

            var jsonContent = await (await _httpService.GetClientAsync()).GetStringAsync("https://api.isthereanydeal.com/v01/game/overview/?key=" + _configuration["IsthereanydealAPIToken"] + "&region=cn&country=CN&shop=steam&ids=app%2F" + steam.Link + "&allowed=steam");

            var thirdResult = JObject.Parse(jsonContent);
            var steamNowJson = new SteamNowJson();
            var steamLowestJson = new SteamLowestJson();

            if (thirdResult["data"]["app/" + steam.Link]["lowest"].Any())
            {
                steamLowestJson = thirdResult["data"]["app/" + steam.Link]["lowest"].ToObject<SteamLowestJson>();
            }
            JObject officialResult = null;
            try
            {
                //尝试使用官方api获取信息
                jsonContent = await (await _httpService.GetClientAsync()).GetStringAsync("https://store.steampowered.com/api/appdetails/?appids=" + steam.Link + "&cc=cn&filters=price_overview");
                officialResult = JObject.Parse(jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Id:{id} 获取Steam官方API数据失败", steam.Link);
            }

            if (officialResult != null && officialResult[steam.Link.ToString()]["success"].ToObject<bool>() == true)
            {
                if (officialResult[steam.Link.ToString()]["data"].Any())
                {
                    var discount_percent = officialResult[steam.Link.ToString()]["data"]["price_overview"]["discount_percent"].ToObject<string>();
                    var final = officialResult[steam.Link.ToString()]["data"]["price_overview"]["final"].ToObject<string>();
                    var final_formatted = officialResult[steam.Link.ToString()]["data"]["price_overview"]["final_formatted"].ToObject<string>();

                    steamNowJson = new SteamNowJson
                    {
                        price = int.Parse(final)*0.01,
                        cut = int.Parse(discount_percent),
                        price_formatted = final_formatted
                    };

                }
            }
            else
            {
                if (thirdResult["data"]["app/" + steam.Link]["price"].Any())
                {
                    steamNowJson = thirdResult["data"]["app/" + steam.Link]["price"].ToObject<SteamNowJson>();
                }
            }

            //更新数据 支持小数点 将真实价格*100储存 即1500表示15元

            //当前价格
            if (steamNowJson.price == null)
            {
                //判断是否 无法获取数据
                if (steamLowestJson.price != null)
                {
                    //已发布
                    steam.State = StoreState.OnSale;

                    steam.PriceNow =null;
                    steam.CutNow =null;

                    steam.CutLowest = steamLowestJson.cut;
                    steam.PriceLowest = steamLowestJson.price;

                    //计算原价
                    if (steam.CutLowest == 100)
                    {
                        steam.OriginalPrice = steam.PriceLowest;
                    }
                    steam.OriginalPrice = (int)(steam.PriceLowest / (1 - ((double)steam.CutLowest / 100)));


                }
                else
                {
                    //未发布
                    steam.State = StoreState.NotPublished;

                    steam.PriceNow = null;
                    steam.CutNow = null;

                    steam.PriceLowest = null;
                    steam.CutLowest = null;
                }
            }
            else
            {
                //已发布
                steam.State = StoreState.OnSale;

                steam.PriceNow = steamNowJson.price;

                steam.CutNow = steamNowJson.cut;

                steam.CutLowest = steamLowestJson.cut;

                //计算原价
                //当前价格 = 原价 * （1 - 折扣）
                //原价 = 当前价格 / （1 - 折扣）
                if (steam.CutNow == 100)
                {
                    steam.OriginalPrice = steam.PriceNow;
                }
                steam.OriginalPrice = (int)(steam.PriceNow / (1 - ((double)steam.CutNow / 100)));

                //计算史低价格
                if (steamLowestJson.price != null)
                {
                    steam.PriceLowest = steam.CutLowest >= 0 ? (int)(steam.OriginalPrice * (1 - ((double)steam.CutLowest / 100))) : steam.OriginalPrice;

                    //比较是否偏差较大
                    if (Math.Abs((steam.PriceLowest.Value / 100) - steamLowestJson.price.Value) > 2)
                    {
                        steam.PriceLowest = steamLowestJson.price;
                    }
                }
                else
                {
                    steam.PriceLowest = null;
                    steam.CutLowest = null;
                }
            }
        }



        #endregion
    }
}
