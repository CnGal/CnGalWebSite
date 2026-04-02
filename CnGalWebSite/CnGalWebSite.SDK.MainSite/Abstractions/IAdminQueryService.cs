using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Almanacs;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Recommends;
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
    /// 分页查询推荐列表。
    /// </summary>
    Task<SdkResult<QueryResultModel<RecommendOverviewModel>>> GetRecommendsAsync(
        QueryParameterModel parameter,
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
}
