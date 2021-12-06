using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Home;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Home
{
    public interface IHomeService
    {
        /// <summary>
        /// 获取即将发售游戏
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeNewestGameViewAsync();
        /// <summary>
        /// 获取近期编辑的游戏或制作组
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeRecentEditViewAsync();

        /// <summary>
        /// 获取近期发售的游戏
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeRecentIssuelGameViewAsync();
        /// <summary>
        /// 获取 优先级最高的游戏或制作组
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeFriendEntriesViewAsync();


        /// <summary>
        /// 获取友情链接 
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeFriendLinksViewAsync();

        /// <summary>
        /// 获取通知
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeNoticesViewAsync();
        /// <summary>
        /// 获取最近发布的文章
        /// </summary>
        /// <returns></returns>
        Task<List<EntryHomeAloneViewModel>> GetHomeArticlesViewAsync();

        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <returns></returns>
        Task<List<Carousel>> GetHomeCarouselsViewAsync();
        /// <summary>
        /// 获取动态
        /// </summary>
        /// <returns></returns>
        Task<List<HomeNewsAloneViewModel>> GetHomeNewsViewAsync();

    }
}
