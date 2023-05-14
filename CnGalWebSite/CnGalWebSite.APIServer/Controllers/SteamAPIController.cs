using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.Helper.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/steam/[action]")]
    public class SteamAPIController : ControllerBase
    {

        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<SteamInfor, int> _steamInforRepository;
        private readonly IAppHelper _appHelper;
        private readonly IHttpService _httpService;
        private readonly ISteamInforService _steamInforService;
        private readonly IRepository<ApplicationUser, string> _userRepository;

        public SteamAPIController(IRepository<SteamInfor, int> steamInforRepository, ISteamInforService steamInforService, IRepository<Tag, int> tagRepository,
        IAppHelper appHelper, IRepository<Entry, int> entryRepository, IRepository<ApplicationUser, string> userRepository, IHttpService httpService)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _steamInforRepository = steamInforRepository;
            _steamInforService = steamInforService;
            _userRepository = userRepository;
            _tagRepository = tagRepository;
            _httpService = httpService;
        }

        [AllowAnonymous]
        [HttpGet("{steamId}")]
        public async Task<string> GetSteamHtml(int steamId)
        {
            var content = await (await _httpService.GetClientAsync()).GetStringAsync($"https://store.steampowered.com/app/{steamId}?l=schinese");
            return content;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<List<SteamUserInfor>>> GetUserSteamInforAsync(string id)
        {
            //获取当前用户ID
            var user = await _appHelper.GetAPICurrentUserAsync(HttpContext);

            var objectUser = await _userRepository.FirstOrDefaultAsync(s => s.Id == id);

            if (objectUser.IsShowGameRecord == false && id != user.Id)
            {
                return NotFound("该用户的游玩记录未公开");
            }

            if (string.IsNullOrWhiteSpace(objectUser.SteamId))
            {
                return new List<SteamUserInfor>();
            }

            var steamids = objectUser.SteamId.Replace("，", ",").Replace("、", ",").Split(',');

            var model = await _steamInforService.GetSteamUserInfors(steamids.ToList());

            return model;

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<EntryInforTipViewModel>>> GetFreeGamesAsync()
        {
            var entryIds = await _tagRepository.GetAll().AsNoTracking()
                .Include(s => s.Entries)
                .Where(s => s.Name == "免费")
                .Select(s => s.Entries.Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s => s.Id).ToList())
                .FirstOrDefaultAsync();

            entryIds = entryIds.ToList().Random().Take(30).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking().Where(s => entryIds.Contains(s.Id)).ToListAsync();

            var model = new List<EntryInforTipViewModel>();
            foreach (var entry in entries)
            {
                model.Add(_appHelper.GetEntryInforTipViewModel(entry));
            }

            return model;
        }


    }
}
