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
            var steams = await _steamInforRepository.GetAll().Select(s => s.SteamId).ToListAsync();

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
            var steamLowestJson = thirdResult["data"]["app/" + steamId]["lowest"].ToObject<SteamLowestJson>();

            try
            {
                //尝试使用官方api获取信息
                jsonContent = await client.GetStringAsync("https://store.steampowered.com/api/appdetails/?appids=" + steamId + "&cc=cn&filters=price_overview");
                var officialResult = JObject.Parse(jsonContent);
                try
                {
                    var discount_percent = officialResult[steamId.ToString()]["data"]["price_overview"]["discount_percent"].ToObject<string>();
                    var final = officialResult[steamId.ToString()]["data"]["price_overview"]["final"].ToObject<string>();
                    var final_formatted = officialResult[steamId.ToString()]["data"]["price_overview"]["final_formatted"].ToObject<string>();

                    steamNowJson = new SteamNowJson
                    {
                        price = int.Parse(final) / 100,
                        cut = int.Parse(discount_percent),
                        price_formatted = final_formatted
                    };
                }
                catch
                {

                    steamNowJson = thirdResult["data"]["app/" + steamId]["price"].ToObject<SteamNowJson>();


                }
            }
            catch
            {
                steamNowJson = new SteamNowJson
                {
                    price = -1,
                    cut = -1
                };
            }


            if (steamNowJson == null)
            {
                steamNowJson = new SteamNowJson();
            }
            if (steamLowestJson == null)
            {
                steamLowestJson = new SteamLowestJson();
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
            //ImagePath = "https://media.st.dl.pinyuncloud.com/steam/apps/" + steamId + "/header.jpg",
            steam.SteamId = steamId;
            steam.PriceLowestString = steamLowestJson.price_formatted.Replace("¥ ", "¥").Replace("¥", "¥ ");
            steam.PriceLowest = steamLowestJson.price;
            steam.CutLowest = steamLowestJson.cut;
            steam.LowestTime = ToolHelper.GetDateTimeFrom1970Ticks(steamLowestJson.recorded);
            steam.PriceNow = steamNowJson.price;
            steam.PriceNowString = "¥ " + steamNowJson.price;// steamNowJson.price_formatted.Replace("¥ ", "¥").Replace("¥", "¥ ");
            steam.CutNow = steamNowJson.cut;

            steam.UpdateTime = DateTime.Now.ToCstTime();
            steam.OriginalPrice = steamNowJson.cut == 0 ? (steamNowJson.price == 0 ? ((steamLowestJson.cut == 0 || steamLowestJson.cut == 100) ? 0 : (int)(steamLowestJson.price / (1 - ((double)steamLowestJson.cut) / 100))) : steamNowJson.price) : (int)(steamNowJson.price / (1 - ((double)steamNowJson.cut) / 100));
            if (steam.PriceNow == 0)
            {
                steam.PriceNow = steam.OriginalPrice;
                steam.PriceNowString = "¥ " + steam.OriginalPrice;
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
