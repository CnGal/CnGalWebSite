using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Almanacs;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Recommends;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;

using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IAdminQueryService
{
    /// <summary>
    /// 获取 API Server 静态概览数据。
    /// </summary>
    Task<SdkResult<ServerStaticOverviewModel>> GetServerOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 API Server 动态概览数据。
    /// </summary>
    Task<SdkResult<ServerRealTimeOverviewModel>> GetServerRealTimeOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取用户数据概览。
    /// </summary>
    Task<SdkResult<UserDataOverviewModel>> GetUserDataOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询评论列表（服务端分页）。
    /// </summary>
    Task<SdkResult<QueryResultModel<CommentOverviewModel>>> GetCommentsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询词条列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<EntryOverviewModel>>> GetEntriesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询文章列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<ArticleOverviewModel>>> GetArticlesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询视频列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<VideoOverviewModel>>> GetVideosAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询标签列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<TagOverviewModel>>> GetTagsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询用户列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<UserOverviewModel>>> GetUsersAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询消息列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<MessageOverviewModel>>> GetMessagesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询收藏夹列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<FavoriteFolderOverviewModel>>> GetFavoriteFoldersAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询备份列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<BackUpArchiveOverviewModel>>> GetBackUpArchivesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询商店信息列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<StoreInfoOverviewModel>>> GetStoreInfoListAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取商店信息编辑模型。
    /// </summary>
    Task<SdkResult<StoreInfoEditModel>> GetStoreInfoEditAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询推荐列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<RecommendOverviewModel>>> GetRecommendsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取推荐编辑模型。
    /// </summary>
    Task<SdkResult<RecommendEditModel>> GetRecommendEditAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询年鉴列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<AlmanacOverviewModel>>> GetAlmanacsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询抽奖列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<LotteryOverviewModel>>> GetLotteriesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询周边列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<PeripheryOverviewModel>>> GetPeripheriesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询投票列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<VoteOverviewModel>>> GetVotesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    // ─── 网站设置 ───

    /// <summary>
    /// 分页查询轮播图列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<CarouselOverviewModel>>> GetCarouselsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询友情链接列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<FriendLinkOverviewModel>>> GetFriendLinksAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    Task<SdkResult<CarouselEditModel>> GetCarouselEditAsync(int id, CancellationToken cancellationToken = default);
    
    Task<SdkResult<FriendLinkEditModel>> GetFriendLinkEditAsync(int id, CancellationToken cancellationToken = default);

    // ─── 动态/周报 ───

    /// <summary>
    /// 分页查询游戏动态列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<GameNewsOverviewModel>>> GetGameNewsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询周报列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<WeeklyNewsOverviewModel>>> GetWeeklyNewsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取单条动态的编辑模型。
    /// </summary>
    Task<SdkResult<EditGameNewsModel>> GetGameNewsEditAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取单条周报的编辑模型。
    /// </summary>
    Task<SdkResult<EditWeeklyNewsModel>> GetWeeklyNewsEditAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取动态预览（文章视图模型）。
    /// </summary>
    Task<SdkResult<ArticleViewModel>> GetGameNewsPreviewAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取周报预览（文章视图模型）。
    /// </summary>
    Task<SdkResult<ArticleViewModel>> GetWeeklyNewsPreviewAsync(
        long id,
        CancellationToken cancellationToken = default);

    // ─── 展会管理 ───

    /// <summary>
    /// 分页查询展会活动列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<ExpoActivityOverviewModel>>> GetExpoActivitiesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询展会票根列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<ExpoTicketOverviewModel>>> GetExpoTicketsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    // ─── 小卖铺 ───

    /// <summary>
    /// 分页查询商品列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<CommodityOverviewModel>>> GetCommoditiesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询兑换码列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<CommodityCodeOverviewModel>>> GetCommodityCodesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    // ─── 词条子功能 ───

    /// <summary>
    /// 分页查询游玩记录列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<GameRecordOverviewModel>>> GetPlayedGamesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询基础信息类型列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<EntryInformationTypeOverviewModel>>> GetEntryInformationTypesAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取基础信息类型编辑模型。
    /// </summary>
    Task<SdkResult<EntryInformationTypeEditModel>> GetEntryInformationTypeEditAsync(
        long id,
        CancellationToken cancellationToken = default);

    // ─── 用户子功能 ───

    /// <summary>
    /// 分页查询头衔列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<RankOverviewModel>>> GetRanksAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取头衔编辑模型。
    /// </summary>
    Task<SdkResult<RankEditModel>> GetRankEditAsync(
        long id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询用户认证列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<UserCertificationOverviewModel>>> GetUserCertificationsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 分页查询操作记录列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<OperationRecordOverviewModel>>> GetOperationRecordsAsync(
        QueryParameterModel parameter,
        CancellationToken cancellationToken = default);

    // ─── 数据图表 ───

    /// <summary>
    /// 获取折线图数据。
    /// </summary>
    Task<SdkResult<LineChartModel>> GetLineChartAsync(
        LineChartType type, DateTime afterTime, DateTime beforeTime,
        CancellationToken cancellationToken = default);
}
