using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.SteamInfors
{
    public interface ISteamInforService
    {

        /// <summary>
        /// 更新所有游戏Steam信息缓存
        /// </summary>
        /// <returns></returns>
        Task UpdateAllGameSteamInfor();
        /// <summary>
        /// 更新所有用户Steam信息缓存
        /// </summary>
        /// <returns></returns>
        Task UpdateAllUserSteamInfor();

        /// <summary>
        /// 获取steam价格信息
        /// </summary>
        /// <param name="steamId">steam平台Id</param>
        /// <param name="entryId">关联的词条Id</param>
        /// <returns></returns>
        Task<SteamInfor> UpdateSteamInfor(int steamId, int entryId);
        /// <summary>
        /// 更新用户的steam相关信息
        /// </summary>
        /// <param name="user">已经更新SteamId的用户</param>
        /// <returns></returns>
        Task<bool> UpdateUserSteam(ApplicationUser user);

        Task<SteamUserInfor> UpdateSteamUserInfor(string SteamId);

        Task<List<SteamUserInfor>> GetSteamUserInfors(List<string> steamids);

        Task<StoreInforViewModel> GetSteamInforAsync(int steamId, int entryId = 0);

        Task BatchUpdateGameSteamInfor(int count);
    }
}
