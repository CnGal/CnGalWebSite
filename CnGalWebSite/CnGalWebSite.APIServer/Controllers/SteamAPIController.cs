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
        [HttpGet("{steamId}/{entryId}")]
        public async Task<ActionResult<StoreInforViewModel>> GetSteamInforAsync(int steamId, int entryId)
        {
            var model = await _steamInforService.GetSteamInforAsync(steamId, entryId);
            return (model == null) ? NotFound() : model;
        }

        [AllowAnonymous]
        [HttpGet("{steamId}")]
        public async Task<ActionResult<StoreInforViewModel>> GetSteamInforAsync(int steamId)
        {
            var model = await _steamInforService.GetSteamInforAsync(steamId);
            return (model == null) ? NotFound() : model;
        }

        [AllowAnonymous]
        [HttpGet("{steamId}")]
        public async Task<string> GetSteamHtml(int steamId)
        {
            var content = await (await _httpService.GetClientAsync()).GetStringAsync($"https://store.steampowered.com/app/{steamId}?l=schinese");
            return content;
        }

        [HttpGet]
        public async Task<ActionResult<Result>> UpdateAllSteamInforAsync()
        {
            try
            {
                await _steamInforService.UpdateAllGameSteamInfor();
                return new Result { Successful = true };
            }
            catch (Exception exc)
            {
                return new Result { Successful = false, Error = exc.Message };
            }

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<SteamInforTipViewModel>>> GetAllDiscountSteamGame()
        {
            var entryIds = await _steamInforRepository.GetAll().Include(s => s.Entry).Where(s => s.CutNow > 0 && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false).Select(s=>s.Id).ToListAsync();
            entryIds = entryIds.ToList().Random().Take(30).ToList();

            var games = await _steamInforRepository.GetAll().Include(s => s.Entry).AsNoTracking().Where(s => entryIds.Contains(s.Id)).ToListAsync();

            var model = new List<SteamInforTipViewModel>();
            foreach (var item in games)
            {
                model.Add(new SteamInforTipViewModel
                {
                    SteamId = item.SteamId,
                    Id = item.EntryId ?? 0,
                    EvaluationCount = item.EvaluationCount,
                    BriefIntroduction = item.Entry.BriefIntroduction,
                    PriceLowestString = item.PriceLowestString,
                    PriceNowString = item.PriceNowString,
                    CutLowest = item.CutLowest,
                    CutNow = item.CutNow,
                    MainImage = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    LowestTime = item.LowestTime,
                    Name = item.Entry.DisplayName,
                    OriginalPrice = item.OriginalPrice,
                    PriceLowest = item.PriceLowest,
                    PriceNow = item.PriceNow,
                    PublishTime = item.Entry.PubulishTime,
                    RecommendationRate = item.RecommendationRate,
                    PlayTime=item.PlayTime
                });
            }

            return model;
        }

        /// <summary>
        /// 获取所有游戏的Steam信息的
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<SteamInforTipViewModel>>> GetAllGameSteamInfors()
        {
            var games = await _steamInforRepository.GetAll().Include(s => s.Entry).Where(s => s.OriginalPrice >= 0 && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false).ToListAsync();

            var model = new List<SteamInforTipViewModel>();
            foreach (var item in games)
            {
                model.Add(new SteamInforTipViewModel
                {
                    SteamId = item.SteamId,
                    Id = item.EntryId ?? 0,
                    EvaluationCount = item.EvaluationCount,
                    BriefIntroduction = item.Entry.BriefIntroduction,
                    PriceLowestString = item.PriceLowestString,
                    PriceNowString = item.PriceNowString,
                    CutLowest = item.CutLowest,
                    CutNow = item.CutNow,
                    MainImage = _appHelper.GetImagePath(item.Entry.MainPicture, "app.png"),
                    LowestTime = item.LowestTime,
                    Name = item.Entry.DisplayName,
                    OriginalPrice = item.OriginalPrice,
                    PriceLowest = item.PriceLowest,
                    PriceNow = item.PriceNow,
                    PublishTime = item.Entry.PubulishTime,
                    RecommendationRate = item.RecommendationRate,
                    PlayTime=item.PlayTime
                });
            }

            return model;
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
                .Select(s => s.Entries.Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.Name) == false).Select(s=>s.Id).ToList())
                .FirstOrDefaultAsync();

            entryIds = entryIds.ToList().Random().Take(30).ToList();

            var entries = await _entryRepository.GetAll().AsNoTracking().Where(s => entryIds.Contains(s.Id)).ToListAsync();

            var model = new List<EntryInforTipViewModel>();
            foreach (var entry in entries)
            {
                model.Add( _appHelper.GetEntryInforTipViewModel(entry));
            }

            return model;
        }


    }
}
