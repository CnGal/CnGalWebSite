using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly HttpClient _httpClient;
        private readonly ILogger<SteamInforService> _logger;

        public SteamInforService(IRepository<SteamInfor, long> steamInforRepository, IRepository<ApplicationUser, string> userRepository, IRepository<Entry, int> entryRepository,
        IConfiguration configuration, IRepository<PlayedGame, long> playedGameRepository, IRepository<SteamUserInfor, long> steamUserInforRepository, ILogger<SteamInforService> logger,
        HttpClient httpClient)
        {
            _steamInforRepository = steamInforRepository;
            _userRepository = userRepository;
            _configuration = configuration;
            _playedGameRepository = playedGameRepository;
            _httpClient = httpClient;
            _steamUserInforRepository = steamUserInforRepository;
            _entryRepository = entryRepository;
            _logger = logger;
        }

        /// <summary>
        /// 更新用户Steam信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
                    var jsonContent = await _httpClient.GetStringAsync(_configuration["SteamAPIUrl"] + "IPlayerService/GetOwnedGames/v1/?key=" + _configuration["SteamAPIToken"] + "&steamid=" + item + "&skip_unvetted_apps=0");
                    var obj = JObject.Parse(jsonContent);
                    var temp = obj["response"].ToObject<UserSteamResponseJson>();
                    steamGames.games.AddRange(temp.games);
                    steamGames.game_count += temp.game_count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "获取用户Steam游戏列表失败");
                    isError = true;
                }
            }


            //查找
            var userGames = await _playedGameRepository.GetAll().Where(s => s.ApplicationUserId == user.Id).ToListAsync();
            var appids = steamGames.games.Select(s => s.appid).Distinct();
            var steams = await _steamInforRepository.GetAll().AsNoTracking()
                .Include(s => s.Entry)
                .Where(s => string.IsNullOrWhiteSpace(s.Entry.Name) == false && s.Entry.IsHidden == false)
                .Where(s => appids.Contains(s.SteamId))
                .Select(s => new
                {
                    s.EntryId,
                    s.SteamId
                })
                .ToListAsync();

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

                _ = await _playedGameRepository.UpdateAsync(item);
            }

            //添加新游戏
            foreach (var item in steams.Where(s => userGames.Select(s => s.EntryId).Contains(s.EntryId) == false))
            {
                //检查是否已经存在
                if (await _playedGameRepository.AnyAsync(s => s.ApplicationUserId == user.Id && s.EntryId == item.EntryId))
                {
                    continue;
                }

                _ = await _playedGameRepository.InsertAsync(new PlayedGame
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
            return !isError;
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


        public async Task<SteamUserInfor> UpdateSteamUserInfor(string SteamId)
        {
            var steamUser = new SteamUserInforJson();
            try
            {
                var jsonContent = await _httpClient.GetStringAsync(_configuration["SteamAPIUrl"] + "ISteamUser/GetPlayerSummaries/v2/?key=" + _configuration["SteamAPIToken"] + "&steamids=" + SteamId);
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
            user ??= new SteamUserInfor();

            user.SteamId = SteamId;
            user.Name = player.personaname;
            user.Image = player.avatarfull.Replace("steamcdn-a.akamaihd.net", "cdn.cloudflare.steamstatic.com");

            _ = user.Id == 0 ? await _steamUserInforRepository.InsertAsync(user) : await _steamUserInforRepository.UpdateAsync(user);
            return user;
        }

    }
    
}
