using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Steam;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.SteamInfors
{
    public interface ISteamInforService
    {
        /// <summary>
        /// 更新用户的steam相关信息
        /// </summary>
        /// <param name="user">已经更新SteamId的用户</param>
        /// <returns></returns>
        Task<bool> UpdateUserSteam(ApplicationUser user);

        Task<List<SteamUserInforModel>> GetSteamUserInfors(List<string> steamids, ApplicationUser user);

        Task BatchUpdateUserSteamInfo(int max);

        Task<bool> CheckUserWishlist(ApplicationUser user, string gameId);
    }
}
