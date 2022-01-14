using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ApiController]
    [Route("api/steam/[action]")]
    public class SteamAPIController : ControllerBase
    {

        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<SteamInfor, int> _steamInforRepository;
        private readonly IAppHelper _appHelper;
        private readonly ISteamInforService _steamInforService;

        public SteamAPIController(IRepository<SteamInfor, int> steamInforRepository, ISteamInforService steamInforService,
        IAppHelper appHelper, IRepository<Entry, int> entryRepository)
        {
            _entryRepository = entryRepository;
            _appHelper = appHelper;
            _steamInforRepository = steamInforRepository;
            _steamInforService = steamInforService;
        }
        [AllowAnonymous]
        [HttpGet("{steamId}/{entryId}")]
        public async Task<ActionResult<SteamInfor>> GetSteamInforAsync(int steamId, int entryId)
        {
            if (await _entryRepository.GetAll().AnyAsync(s => s.Id == entryId) == false)
            {
                return NotFound();
            }
            //尝试到数据库中查找信息
            //没有找到 则尝试更新数据
            //无法更新则返回错误
            var steamInfor = await _steamInforRepository.FirstOrDefaultAsync(s => s.SteamId == steamId);
            if (steamInfor != null /*&& steamInfor.PriceNow != -1*/)
            {
               
                return steamInfor;
            }
            steamInfor = await _steamInforService.UpdateSteamInfor(steamId, entryId);
            if (steamInfor == null)
            {
                return NotFound();
            }

            return steamInfor;
        }
        /// <summary>
        /// 使所有词条的steamId同步到数据库
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<Result>> GetAllEntriesSteamAsync()
        {

            //通过Id获取词条 
            var entries = await _entryRepository.GetAll().Include(s => s.Information).Where(s => s.Type == EntryType.Game && s.IsHidden != true).ToListAsync();

            //循环添加
            foreach (var infor in entries)
            {

                foreach (var item in infor.Information)
                {
                    if (item.Modifier == "基本信息" && item.DisplayName == "Steam平台Id")
                    {
                        try
                        {
                            var steamId = int.Parse(item.DisplayValue);
                            var steamInfor = await _steamInforRepository.FirstOrDefaultAsync(s => s.SteamId == steamId);
                            if (steamInfor == null)
                            {
                                await _steamInforService.UpdateSteamInfor(steamId, infor.Id);
                            }
                            break;
                        }
                        catch
                        {

                        }
                    }
                }
            }
            return new Result { Successful = true };
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

        [HttpGet]
        public async Task<ActionResult<Result>> GetAllSteamImageToGame()
        {
            await _appHelper.GetAllSteamImageToGame();
            return new Result { Successful = true };
        }

    }
}
