using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.SteamInfors
{
    public class SteamInforService : ISteamInforService
    {
        private readonly IRepository<SteamInfor, long> _steamInforRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<PlayedGame, string> _playedGameRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public SteamInforService(IRepository<SteamInfor, long> steamInforRepository, IRepository<ApplicationUser, string> userRepository, IConfiguration configuration, IRepository<PlayedGame, string> playedGameRepository,
             IHttpClientFactory clientFactory)
        {
            _steamInforRepository = steamInforRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _playedGameRepository = playedGameRepository;
            _clientFactory = clientFactory;
        }

        public async Task UpdateAllGameSteamInfor()
        {
            var steams = await _steamInforRepository.GetAll().AsNoTracking().OrderByDescending(s=>s.PriceNow).ThenByDescending(s=>s.EntryId).Select(s => s.SteamId).ToListAsync();

            foreach (var item in steams)
            {
                await UpdateSteamInfor(item, 0);
            }

        }

        public async Task UpdateAllUserSteamInfor()
        {
            var users = await _userRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.SteamId) == false).ToListAsync();

            foreach (var item in users)
            {
                await UpdateUserSteam(item);
            }
        }

        public async Task<SteamInfor> UpdateSteamInfor(int steamId, int entryId)
        {
            //获取信息
            var client = _clientFactory.CreateClient();
            var jsonContent = await client.GetStringAsync("https://api.isthereanydeal.com/v01/game/overview/?key=" + _configuration["IsthereanydealAPIToken"] + "&region=cn&country=CN&shop=steam&ids=app%2F" + steamId + "&allowed=steam");
            var thirdResult = JObject.Parse(jsonContent);
            var steamNowJson = new SteamNowJson
            {
                price = -1,
                cut = -1
            };
            var steamLowestJson = new SteamLowestJson
            {
                price = -1,
                cut = -1
            };
            if (thirdResult["data"]["app/" + steamId]["lowest"].Count() != 0)
            {
                steamLowestJson = thirdResult["data"]["app/" + steamId]["lowest"].ToObject<SteamLowestJson>();
            }

            try
            {
                //尝试使用官方api获取信息
                jsonContent = await client.GetStringAsync("https://store.steampowered.com/api/appdetails/?appids=" + steamId + "&cc=cn&filters=price_overview");
                var officialResult = JObject.Parse(jsonContent);

                if (officialResult[steamId.ToString()]["success"].ToObject<string>() == "True")
                {
                    if (officialResult[steamId.ToString()]["data"].Count() != 0)
                    {
                        var discount_percent = officialResult[steamId.ToString()]["data"]["price_overview"]["discount_percent"].ToObject<string>();
                        var final = officialResult[steamId.ToString()]["data"]["price_overview"]["final"].ToObject<string>();
                        var final_formatted = officialResult[steamId.ToString()]["data"]["price_overview"]["final_formatted"].ToObject<string>();

                        steamNowJson = new SteamNowJson
                        {
                            price = int.Parse(final),
                            cut = int.Parse(discount_percent),
                            price_formatted = final_formatted
                        };

                    }
                    else
                    {
                        steamNowJson = new SteamNowJson
                        {
                            price = 0,
                            cut = 0
                        };
                    }
                }
                else
                {
                    if (thirdResult["data"]["app/" + steamId]["price"].Count() != 0)
                    {
                        steamNowJson = thirdResult["data"]["app/" + steamId]["price"].ToObject<SteamNowJson>();
                    }
                }
            }
            catch
            {

            }


            //修正价无法获取价格的游戏状态
            if (steamNowJson.price == 0 && steamLowestJson.price == -1)
            {
                steamNowJson = new SteamNowJson
                {
                    price = -1,
                    cut = -1
                };
            }

            //获取已存在的信息
            var steam = await _steamInforRepository.FirstOrDefaultAsync(s => s.SteamId == steamId);
            if (steam == null)
            {
                steam = new SteamInfor
                {
                    EntryId = entryId
                };
            }

            //更新数据 支持小数点 将真实价格*100储存 即1500表示15元

            //当前价格
            steam.PriceNow = steamNowJson.price;
            if (steam.PriceNow == -1)
            {
                //判断是否为已下架
                if (steamLowestJson.price != -1)
                {
                    steam.PriceNow = -2;
                    steam.CutNow = -2;

                    steam.CutLowest = steamLowestJson.cut;
                    steam.PriceLowest = steamLowestJson.price*100;
                    steam.PriceLowestString = "¥ " + ((double)steam.PriceLowest / 100).ToString("0.00");
                    steam.LowestTime = ToolHelper.GetDateTimeFrom1970Ticks(steamLowestJson.recorded);

                    //计算原价
                    if (steam.CutLowest == 100)
                    {
                        steam.OriginalPrice = steam.PriceLowest;
                    }
                    steam.OriginalPrice = (int)(steam.PriceLowest / (1 - ((double)steam.CutLowest / 100)));


                }
                else
                {
                    steam.PriceNowString = "¥ 0";
                    steam.CutNow = -1;
                    steam.OriginalPrice = -1;

                    steam.CutLowest = -1;
                    steam.PriceLowest = -1;
                    steam.PriceLowestString = "¥ 0";

                    steam.LowestTime = DateTime.MinValue;
                }
            }
            else
            {
                steam.PriceNowString = "¥ " + ((double)steam.PriceNow / 100).ToString("0.00");
                steam.CutNow = steamNowJson.cut;

                steam.LowestTime = ToolHelper.GetDateTimeFrom1970Ticks(steamLowestJson.recorded);
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
                if (steamLowestJson.price != -1)
                {
                    if (steam.CutLowest >= 0)
                    {
                        steam.PriceLowest = (int)(steam.OriginalPrice * (1 - ((double)steam.CutLowest / 100)));
                    }
                    else
                    {
                        steam.PriceLowest = steam.OriginalPrice;
                    }

                    //比较是否偏差较大
                    if(Math.Abs( steam.PriceLowest/100 -steamLowestJson.price)>2)
                    {
                        steam.PriceLowest = steamLowestJson.price * 100;
                    }

                    steam.PriceLowestString = "¥ " + ((double)steam.PriceLowest / 100).ToString("0.00");
                }
                else
                {
                    steam.PriceLowestString = "¥ 0";
                    steam.PriceLowest = -1;
                }
              

            }

            //临时补正
            if (steamId== 1506340)
            {
                steam.PriceNow = steam.OriginalPrice;
                steam.CutNow = 0;
                steam.PriceNowString = "¥ " + ((double)steam.PriceNow / 100).ToString("0.00");
            }

            if (steam.Id == 0)
            {
                await _steamInforRepository.InsertAsync(steam);
            }
            else
            {
                await _steamInforRepository.UpdateAsync(steam);
            }

            return steam;
        }

        public async Task<bool> UpdateUserSteam(ApplicationUser user)
        {
            //获取最新列表
            //获取信息
            try
            {
                var client = _clientFactory.CreateClient();
                var jsonContent = await client.GetStringAsync("http://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key=" + _configuration["SteamAPIToken"] + "&steamid=" + user.SteamId);
                var obj = JObject.Parse(jsonContent);
                var steamGames = obj["response"].ToObject<UserSteamResponseJson>();
                if (steamGames == null || steamGames.games == null)
                {
                    return false;
                }
                var appids = steamGames.games.Select(s => s.appid);

                //查找
                var steams = await _steamInforRepository.GetAll().Where(s => appids.Contains(s.SteamId)).ToListAsync();
                var userGames = await _playedGameRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).Select(s => s.EntryId).ToListAsync();
                //遍历列表并导入到已玩游戏
                foreach (var item in steams)
                {
                    if (item.EntryId != 0)
                    {
                        //查找用户是否拥有游戏
                        if (userGames.Any(s => s == item.EntryId) == false)
                        {
                            await _playedGameRepository.InsertAsync(new PlayedGame
                            {
                                EntryId = item.EntryId,
                                ApplicationUserId = user.Id
                            });
                        }
                    }
                }

                return true;

            }
            catch
            {
                return false;
            }
        }

    }
}
