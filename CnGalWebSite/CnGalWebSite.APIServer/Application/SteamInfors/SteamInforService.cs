using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IRepository<PlayedGame, long> _playedGameRepository;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<SteamUserInfor, long> _steamUserInforRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public SteamInforService(IRepository<SteamInfor, long> steamInforRepository, IRepository<ApplicationUser, string> userRepository, IRepository<Entry, int> entryRepository,
        IConfiguration configuration, IRepository<PlayedGame, long> playedGameRepository, IRepository<SteamUserInfor, long> steamUserInforRepository,
        IHttpClientFactory clientFactory)
        {
            _steamInforRepository = steamInforRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _playedGameRepository = playedGameRepository;
            _clientFactory = clientFactory;
            _steamUserInforRepository = steamUserInforRepository;
            _entryRepository = entryRepository;
        }

        public async Task<SteamInfor> GetSteamInforAsync(int steamId, int entryId = 0)
        {

            //尝试到数据库中查找信息
            //没有找到 则尝试更新数据
            //无法更新则返回错误
            var steamInfor = await _steamInforRepository.FirstOrDefaultAsync(s => s.SteamId == steamId);
            if (steamInfor != null/* && steamInfor.PriceNow != -1 && steamInfor.PriceNow != -2*/)
            {
                return steamInfor;
            }

            if (await _entryRepository.GetAll().AnyAsync(s => s.Id == entryId) == false)
            {
                return null;
            }

            steamInfor = await UpdateSteamInfor(steamId, entryId);
            if (steamInfor == null)
            {
                return null;
            }

            return steamInfor;
        }

        public async Task UpdateAllGameSteamInfor()
        {
            var steams = await _steamInforRepository.GetAll().AsNoTracking().OrderByDescending(s => s.PriceNow).ThenByDescending(s => s.EntryId).Select(s => s.SteamId).ToListAsync();

            foreach (var item in steams)
            {
                await UpdateSteamInfor(item, 0);
            }

        }

        public async Task UpdateAllUserSteamInfor()
        {
            var steamIds = await _steamUserInforRepository.GetAll().Where(s => string.IsNullOrWhiteSpace(s.SteamId) == false).Select(s => s.SteamId).ToListAsync();

            foreach (var item in steamIds)
            {
                await UpdateSteamUserInfor(item);
            }
        }

        public async Task<SteamInfor> UpdateSteamInfor(int steamId, int entryId)
        {
            //获取信息
            using var client = _clientFactory.CreateClient();

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


            //修正无法获取价格的游戏状态
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
                    EntryId = entryId,
                    SteamId = steamId,
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
                    steam.PriceLowest = steamLowestJson.price * 100;
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
                    if (Math.Abs(steam.PriceLowest / 100 - steamLowestJson.price) > 2)
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
            if (steamId == 1506340)
            {
                steam.PriceNow = steam.OriginalPrice;
                steam.CutNow = 0;
                steam.PriceNowString = "¥ " + ((double)steam.PriceNow / 100).ToString("0.00");
            }
            if (steamId == 1827680 || steamId == 1903370 || steamId == 1874810 || steamId == 1933640 || steamId == 1840590 || steamId == 1988630)
            {
                steam.PriceNow = 0;
                steam.CutNow = 0;
                steam.PriceNowString = "¥ 0.00";
            }


            //获取评测
            if (steam.PriceNow >= 0)
            {
                try
                {
                    var item = await GetSteamEvaluationAsync(steamId);
                    steam.EvaluationCount = item.EvaluationCount;
                    steam.RecommendationRate = item.RecommendationRate;
                }
                catch (Exception)
                {

                }
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
            var now = DateTime.Now.ToCstTime();
            var steamIds = new string[0];
            if (string.IsNullOrWhiteSpace(user.SteamId) == false)
            {
                steamIds = user.SteamId.Replace("，", ",").Replace("、", ",").Split(',');
            }
            //获取最新列表
            //获取信息
            var steamGames = new UserSteamResponseJson();

            var isError = false;
            foreach (var item in steamIds)
            {
                try
                {
                    using var client = _clientFactory.CreateClient();
                    var jsonContent = await client.GetStringAsync("https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key=" + _configuration["SteamAPIToken"] + "&steamid=" + item);
                    var obj = JObject.Parse(jsonContent);
                    var temp = obj["response"].ToObject<UserSteamResponseJson>();
                    steamGames.games.AddRange(temp.games);
                    steamGames.game_count += temp.game_count;
                }
                catch (Exception)
                {
                    isError = true;
                }
            }


            //查找
            var userGames = await _playedGameRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).ToListAsync();
            var appids = steamGames.games.Select(s => s.appid).Distinct();
            var steams = await _steamInforRepository.GetAll().Where(s => appids.Contains(s.SteamId)).ToListAsync();

            //遍历列表更新已玩游戏信息
            foreach (var item in userGames)
            {
                var steamTemp = steams.FirstOrDefault(s => s.EntryId == item.EntryId);
                if (steamTemp != null)
                {
                    item.IsInSteam = true;
                    item.PlayDuration = steamGames.games.FirstOrDefault(s => s.appid == steamTemp.SteamId)?.playtime_forever ?? 0;
                }
                else
                {
                    item.IsInSteam = false;
                }

                await _playedGameRepository.UpdateAsync(item);
            }

            //添加新游戏
            foreach (var item in steams.Where(s => userGames.Select(s => s.EntryId).Contains(s.EntryId) == false))
            {
                await _playedGameRepository.InsertAsync(new PlayedGame
                {
                    IsInSteam = true,
                    PlayDuration = steamGames.games.FirstOrDefault(s => s.appid == item.SteamId)?.playtime_forever ?? 0,
                    EntryId = item.EntryId,
                    Type = ((steamGames.games.FirstOrDefault(s => s.appid == item.SteamId)?.playtime_forever ?? 0) > 0) ? PlayedGameType.Played : PlayedGameType.UnPlayed,
                    ApplicationUserId = user.Id,
                    ShowPublicly = true,
                    LastEditTime = now
                });
            }


            if (isError)
            {
                return false;
            }
            else
            {
                return true;
            }



        }

        public async Task<SteamEvaluation> GetSteamEvaluationAsync(int id)
        {
            using var client = _clientFactory.CreateClient();

            var content = await client.GetStringAsync("https://store.steampowered.com/app/" + id);

            var document = new HtmlDocument();
            document.LoadHtml(content);

            var node = document.GetElementbyId("userReviews");
            var text = "";
            if (node.ChildNodes.Count > 3)
            {
                text = node.ChildNodes[3].ChildNodes[3].ChildNodes[5].InnerText;

            }
            else
            {
                text = node.ChildNodes[1].ChildNodes[3].ChildNodes[5].InnerText;

            }



            var countStr = ToolHelper.MidStrEx(text, "the ", " ").Replace(",", "");
            var rateStr = ToolHelper.MidStrEx(text, " ", "% ");
            return new SteamEvaluation
            {
                EvaluationCount = int.Parse(countStr),
                RecommendationRate = int.Parse(rateStr),
            };
        }

        public async Task<SteamUserInfor> UpdateSteamUserInfor(string SteamId)
        {
            var steamUser = new SteamUserInforJson();
            try
            {
                using var client = _clientFactory.CreateClient();
                var jsonContent = await client.GetStringAsync("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + _configuration["SteamAPIToken"] + "&steamids=" + SteamId);
                var obj = JObject.Parse(jsonContent);
                steamUser = obj.ToObject<SteamUserInforJson>();
            }
            catch (Exception)
            {
                return null;
            }
            if (steamUser.response.players.Count == 0)
            {
                return null;
            }

            var player = steamUser.response.players[0];
            var user = await _steamUserInforRepository.FirstOrDefaultAsync(s => s.SteamId == SteamId);
            if (user == null)
            {
                user = new SteamUserInfor();
            }

            user.SteamId = SteamId;
            user.Name = player.personaname;
            user.Image = player.avatarfull.Replace("steamcdn-a.akamaihd.net", "cdn.cloudflare.steamstatic.com");

            if (user.Id == 0)
            {
                await _steamUserInforRepository.InsertAsync(user);
            }
            else
            {
                await _steamUserInforRepository.UpdateAsync(user);
            }
            return user;
        }

        public async Task<List<SteamUserInfor>> GetSteamUserInfors(List<string> steamids)
        {
            var model = await _steamUserInforRepository.GetAllListAsync(s => steamids.Contains(s.SteamId));

            foreach (var item in steamids)
            {
                //不存在则获取
                if (model.Any(s => s.SteamId == item) == false)
                {
                    var temp = await UpdateSteamUserInfor(item);
                    if (temp != null)
                    {
                        model.Add(temp);
                    }
                }
            }

            return model;
        }

    }
}
