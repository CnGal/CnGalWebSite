using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Home;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Home
{
    public interface IHomeService
    {
        Task<List<RecentlyDemoGameItemModel>> ListRecentlyDemoGames();
        /// <summary>
        /// 获取近期发售的游戏
        /// </summary>
        /// <returns></returns>
        Task<List<PublishedGameItemModel>> ListPublishedGames();
        /// <summary>
        /// 获取近期编辑的游戏或制作组
        /// </summary>
        /// <returns></returns>
        Task<List<RecentlyEditedGameItemModel>> ListRecentlyEditedGames();

        /// <summary>
        /// 获取即将发售游戏
        /// </summary>
        /// <returns></returns>
        Task<List<UpcomingGameItemModel>> ListUpcomingGames();
        /// <summary>
        /// 获取友情链接 
        /// </summary>
        /// <returns></returns>
        Task<List<FriendLinkItemModel>> ListFriendLinks();

        /// <summary>
        /// 获取通知
        /// </summary>
        /// <returns></returns>
        Task<List<AnnouncementItemModel>> ListAnnouncements();
        /// <summary>
        /// 获取最近发布的文章
        /// </summary>
        /// <returns></returns>
        Task<List<LatestArticleItemModel>> ListLatestArticles();

        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <returns></returns>
        Task<List<CarouselViewModel>> GetHomeCarouselsViewAsync();
        /// <summary>
        /// 获取动态
        /// </summary>
        /// <returns></returns>
        Task<List<HomeNewsAloneViewModel>> GetHomeNewsViewAsync();

        Task<List<LatestVideoItemModel>> ListLatestVideos();
    }
}
