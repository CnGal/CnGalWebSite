using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Recommends;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IAdminCommandService
{
    /// <summary>
    /// 刷新搜索缓存（重建 ES 索引）。
    /// </summary>
    Task<SdkResult<bool>> RefreshSearchDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行预置的临时脚本。
    /// </summary>
    Task<SdkResult<bool>> RunTempFunctionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 调整评论优先级。
    /// </summary>
    Task<SdkResult<bool>> EditCommentPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);

    /// <summary>
    /// 隐藏或显示评论。
    /// </summary>
    Task<SdkResult<bool>> HideCommentAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);

    // ─── 词条操作 ───

    /// <summary>
    /// 调整词条优先级。
    /// </summary>
    Task<SdkResult<bool>> EditEntryPriorityAsync(int[] ids, int plusPriority, CancellationToken cancellationToken = default);

    /// <summary>
    /// 隐藏或显示词条。
    /// </summary>
    Task<SdkResult<bool>> HideEntryAsync(int[] ids, bool isHidden, CancellationToken cancellationToken = default);

    /// <summary>
    /// 隐藏或显示词条外链。
    /// </summary>
    Task<SdkResult<bool>> HideEntryOutlinkAsync(int[] ids, bool isHidden, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置词条可否评论。
    /// </summary>
    Task<SdkResult<bool>> EditEntryCanCommentAsync(int[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 游玩记录操作 ───

    /// <summary>
    /// 隐藏或显示游玩记录。
    /// </summary>
    Task<SdkResult<bool>> HidePlayedGameAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);

    /// <summary>
    /// 公开或隐藏游玩记录的公开状态。
    /// </summary>
    Task<SdkResult<bool>> ShowPubliclyPlayedGameAsync(long[] ids, bool showPublicly, CancellationToken cancellationToken = default);

    // ─── 文章操作 ───

    /// <summary>
    /// 调整文章优先级。
    /// </summary>
    Task<SdkResult<bool>> EditArticlePriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);

    /// <summary>
    /// 隐藏或显示文章。
    /// </summary>
    Task<SdkResult<bool>> HideArticleAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置文章可否评论。
    /// </summary>
    Task<SdkResult<bool>> EditArticleCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 视频操作 ───

    Task<SdkResult<bool>> EditVideoPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> HideVideoAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> EditVideoCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 标签操作 ───

    Task<SdkResult<bool>> HideTagAsync(int[] ids, bool isHidden, CancellationToken cancellationToken = default);

    // ─── 抽奖操作 ───

    Task<SdkResult<bool>> EditLotteryPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> HideLotteryAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> EditLotteryCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 周边操作 ───

    Task<SdkResult<bool>> EditPeripheryPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> HidePeripheryAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> EditPeripheryCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 投票操作 ───

    Task<SdkResult<bool>> EditVotePriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> HideVoteAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> EditVoteCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 用户操作 ───

    Task<SdkResult<bool>> EditSpaceCanCommentAsync(string[] ids, bool canComment, CancellationToken cancellationToken = default);

    // ─── 头衔操作 ───

    Task<SdkResult<bool>> EditRankAsync(RankEditModel model, CancellationToken cancellationToken = default);

    Task<SdkResult<bool>> EditRankPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default);

    Task<SdkResult<bool>> HideRankAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default);

    // ─── 推荐操作 ───

    Task<SdkResult<bool>> EditRecommendAsync(RecommendEditModel model, CancellationToken cancellationToken = default);

    // ─── 商店信息操作 ───

    Task<SdkResult<bool>> EditStoreInfoAsync(StoreInfoEditModel model, CancellationToken cancellationToken = default);

    // ─── 基础信息类型操作 ───

    Task<SdkResult<bool>> EditEntryInformationTypeAsync(EntryInformationTypeEditModel model, CancellationToken cancellationToken = default);

    // ─── 备份操作 ───

    Task<SdkResult<bool>> RunBackUpArchiveAsync(long[] ids, CancellationToken cancellationToken = default);

    // ─── 轮播图操作 ───

    Task<SdkResult<bool>> EditCarouselPriorityAsync(int[] ids, int plusPriority, CancellationToken cancellationToken = default);

    Task<SdkResult<bool>> EditCarouselAsync(CarouselEditModel model, CancellationToken cancellationToken = default);

    // ─── 友情链接操作 ───

    Task<SdkResult<bool>> EditFriendLinkPriorityAsync(int[] ids, int plusPriority, CancellationToken cancellationToken = default);

    Task<SdkResult<bool>> EditFriendLinkAsync(FriendLinkEditModel model, CancellationToken cancellationToken = default);

    // ─── 动态操作 ───

    /// <summary>
    /// 编辑动态。
    /// </summary>
    Task<SdkResult<bool>> EditGameNewsAsync(EditGameNewsModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 添加自定义动态。
    /// </summary>
    Task<SdkResult<bool>> AddCustomNewsAsync(EditGameNewsModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布动态为文章。
    /// </summary>
    Task<SdkResult<bool>> PublishGameNewsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 忽略或恢复动态。
    /// </summary>
    Task<SdkResult<bool>> IgnoreGameNewsAsync(long[] ids, bool isIgnore, CancellationToken cancellationToken = default);

    /// <summary>
    /// 从微博链接导入动态。
    /// </summary>
    Task<SdkResult<bool>> AddWeiboNewsAsync(string link, CancellationToken cancellationToken = default);

    // ─── 周报操作 ───

    /// <summary>
    /// 编辑周报。
    /// </summary>
    Task<SdkResult<bool>> EditWeeklyNewsAsync(EditWeeklyNewsModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发布周报为文章。
    /// </summary>
    Task<SdkResult<bool>> PublishWeeklyNewsAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 重置周报内容。
    /// </summary>
    Task<SdkResult<bool>> ResetWeeklyNewsAsync(long id, CancellationToken cancellationToken = default);

    // ─── 展会操作 ───

    /// <summary>
    /// 编辑/新增展会活动。
    /// </summary>
    Task<SdkResult<bool>> EditExpoActivityAsync(ExpoActivityEditModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 编辑/新增展会票根。
    /// </summary>
    Task<SdkResult<bool>> EditExpoTicketAsync(ExpoTicketEditModel model, CancellationToken cancellationToken = default);

    // ─── 商品操作 ───

    /// <summary>
    /// 编辑/新增商品。
    /// </summary>
    Task<SdkResult<bool>> EditCommodityAsync(CommodityEditModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 编辑/新增兑换码。
    /// </summary>
    Task<SdkResult<bool>> EditCommodityCodeAsync(CommodityCodeEditModel model, CancellationToken cancellationToken = default);
}
