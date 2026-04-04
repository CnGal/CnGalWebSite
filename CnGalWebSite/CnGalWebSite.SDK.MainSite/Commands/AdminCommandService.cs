using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Votes;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class AdminCommandService(HttpClient httpClient, ILogger<AdminCommandService> logger)
    : CommandServiceBase(httpClient), IAdminCommandService
{
    protected override ILogger Logger => logger;

    // ─── 通用 POST 命令辅助方法 ───

    private async Task<SdkResult<bool>> PostCommandAsync<TBody>(
        string apiPath, TBody body, string errorCode, string displayName,
        CancellationToken cancellationToken) where TBody : class
    {
        try
        {
            var result = await PostAsJsonAsync<TBody, Result>(apiPath, body, cancellationToken);
            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail($"ADMIN_{errorCode}_FAILED", result?.Error ?? $"{displayName}失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{DisplayName}异常。BaseAddress={BaseAddress}", displayName, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail($"ADMIN_{errorCode}_EXCEPTION", $"{displayName}时发生异常");
        }
    }

    private async Task<SdkResult<bool>> GetCommandAsync(
        string apiPath, string errorCode, string displayName,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await GetFromJsonAsync<Result>(apiPath, cancellationToken);
            if (result is null || !result.Successful)
            {
                return SdkResult<bool>.Fail($"ADMIN_{errorCode}_FAILED", result?.Error ?? $"{displayName}失败");
            }
            return SdkResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{DisplayName}异常。BaseAddress={BaseAddress}", displayName, HttpClient.BaseAddress);
            return SdkResult<bool>.Fail($"ADMIN_{errorCode}_EXCEPTION", $"{displayName}时发生异常");
        }
    }

    // ─── 系统 ───

    public Task<SdkResult<bool>> RefreshSearchDataAsync(CancellationToken cancellationToken = default)
        => GetCommandAsync("api/admin/RefreshSearchData", "REFRESH_SEARCH", "刷新搜索缓存", cancellationToken);

    public Task<SdkResult<bool>> RunTempFunctionAsync(CancellationToken cancellationToken = default)
        => GetCommandAsync("api/admin/TempFunction", "TEMP_FUNCTION", "执行临时脚本", cancellationToken);

    // ─── 评论 ───

    public Task<SdkResult<bool>> EditCommentPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditCommentPriority",
            new EditCommentPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_COMMENT_PRIORITY", "调整评论优先级", cancellationToken);

    public Task<SdkResult<bool>> HideCommentAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/HiddenComment",
            new HiddenCommentModel { Ids = ids, IsHidden = isHidden },
            "HIDE_COMMENT", "操作评论", cancellationToken);

    // ─── 词条 ───

    public Task<SdkResult<bool>> EditEntryPriorityAsync(int[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/entries/EditEntryPriority",
            new EditEntryPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_ENTRY_PRIORITY", "调整词条优先级", cancellationToken);

    public Task<SdkResult<bool>> HideEntryAsync(int[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/entries/HiddenEntry",
            new HiddenEntryModel { Ids = ids, IsHidden = isHidden },
            "HIDE_ENTRY", "操作词条显隐", cancellationToken);

    public Task<SdkResult<bool>> HideEntryOutlinkAsync(int[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/entries/HideEntryOutlink",
            new HiddenEntryModel { Ids = ids, IsHidden = isHidden },
            "HIDE_ENTRY_OUTLINK", "操作词条外链", cancellationToken);

    public Task<SdkResult<bool>> EditEntryCanCommentAsync(int[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditEntryCanComment",
            new EditEntryCanCommentModel { Ids = ids, CanComment = canComment },
            "EDIT_ENTRY_CAN_COMMENT", "设置词条留言板", cancellationToken);

    // ─── 文章 ───

    public Task<SdkResult<bool>> EditArticlePriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/articles/EditPriority",
            new EditArticlePriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_ARTICLE_PRIORITY", "调整文章优先级", cancellationToken);

    public Task<SdkResult<bool>> HideArticleAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/articles/Hide",
            new HiddenArticleModel { Ids = ids, IsHidden = isHidden },
            "HIDE_ARTICLE", "操作文章显隐", cancellationToken);

    public Task<SdkResult<bool>> EditArticleCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditArticleCanComment",
            new EditArticleCanCommentModel { Ids = ids, CanComment = canComment },
            "EDIT_ARTICLE_CAN_COMMENT", "设置文章留言板", cancellationToken);

    // ─── 视频 ───

    public Task<SdkResult<bool>> EditVideoPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/videos/EditPriority",
            new EditArticlePriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_VIDEO_PRIORITY", "调整视频优先级", cancellationToken);

    public Task<SdkResult<bool>> HideVideoAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/videos/Hide",
            new HiddenArticleModel { Ids = ids, IsHidden = isHidden },
            "HIDE_VIDEO", "操作视频显隐", cancellationToken);

    public Task<SdkResult<bool>> EditVideoCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditVideoCanComment",
            new EditArticleCanCommentModel { Ids = ids, CanComment = canComment },
            "EDIT_VIDEO_CAN_COMMENT", "设置视频留言板", cancellationToken);

    // ─── 标签 ───

    public Task<SdkResult<bool>> HideTagAsync(int[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/Tags/HiddenTag",
            new HiddenTagModel { Ids = ids, IsHidden = isHidden },
            "HIDE_TAG", "操作标签显隐", cancellationToken);

    // ─── 抽奖 ───

    public Task<SdkResult<bool>> EditLotteryPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/Lotteries/EditLotteryPriority",
            new EditLotteryPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_LOTTERY_PRIORITY", "调整抽奖优先级", cancellationToken);

    public Task<SdkResult<bool>> HideLotteryAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/Lotteries/HiddenLottery",
            new HiddenLotteryModel { Ids = ids, IsHidden = isHidden },
            "HIDE_LOTTERY", "操作抽奖显隐", cancellationToken);

    public Task<SdkResult<bool>> EditLotteryCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditLotteryCanComment",
            new EditLotteryCanCommentModel { Ids = ids, CanComment = canComment },
            "EDIT_LOTTERY_CAN_COMMENT", "设置抽奖留言板", cancellationToken);

    // ─── 周边 ───

    public Task<SdkResult<bool>> EditPeripheryPriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/peripheries/EditPeripheryPriority",
            new EditPeripheryPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_PERIPHERY_PRIORITY", "调整周边优先级", cancellationToken);

    public Task<SdkResult<bool>> HidePeripheryAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/peripheries/HiddenPeriphery",
            new HiddenPeripheryModel { Ids = ids, IsHidden = isHidden },
            "HIDE_PERIPHERY", "操作周边显隐", cancellationToken);

    public Task<SdkResult<bool>> EditPeripheryCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditPeripheryCanComment",
            new EditPeripheryCanCommentModel { Ids = ids, CanComment = canComment },
            "EDIT_PERIPHERY_CAN_COMMENT", "设置周边留言板", cancellationToken);

    // ─── 投票 ───

    public Task<SdkResult<bool>> EditVotePriorityAsync(long[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/votes/EditVotePriority",
            new EditVotePriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_VOTE_PRIORITY", "调整投票优先级", cancellationToken);

    public Task<SdkResult<bool>> HideVoteAsync(long[] ids, bool isHidden, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/votes/HiddenVote",
            new HiddenVoteModel { Ids = ids, IsHidden = isHidden },
            "HIDE_VOTE", "操作投票显隐", cancellationToken);

    public Task<SdkResult<bool>> EditVoteCanCommentAsync(long[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditVoteCanComment",
            new EditVoteCanCommentModel { Ids = ids, CanComment = canComment },
            "EDIT_VOTE_CAN_COMMENT", "设置投票留言板", cancellationToken);

    // ─── 用户 ───

    public Task<SdkResult<bool>> EditSpaceCanCommentAsync(string[] ids, bool canComment, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/comments/EditSpaceCanComment",
            new EditSpaceCanComment { Ids = ids, CanComment = canComment },
            "EDIT_SPACE_CAN_COMMENT", "设置用户留言板", cancellationToken);

    // ─── 备份 ───

    public Task<SdkResult<bool>> RunBackUpArchiveAsync(long[] ids, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/BackUpArchives/RunBackUpArchive",
            new RunBackUpArchiveModel { Ids = ids },
            "RUN_BACKUP", "执行备份", cancellationToken);

    // ─── 轮播图 ───

    public Task<SdkResult<bool>> EditCarouselPriorityAsync(int[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/home/EditCarouselPriority",
            new EditEntryPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_CAROUSEL_PRIORITY", "调整轮播图优先级", cancellationToken);

    public Task<SdkResult<bool>> EditCarouselAsync(CarouselEditModel model, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/home/EditCarousel", model, "EDIT_CAROUSEL", "编辑轮播图", cancellationToken);

    // ─── 友情链接 ───

    public Task<SdkResult<bool>> EditFriendLinkPriorityAsync(int[] ids, int plusPriority, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/home/EditFriendLinkPriority",
            new EditEntryPriorityViewModel { Ids = ids, PlusPriority = plusPriority },
            "EDIT_FRIENDLINK_PRIORITY", "调整友情链接优先级", cancellationToken);

    public Task<SdkResult<bool>> EditFriendLinkAsync(FriendLinkEditModel model, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/home/EditFriendLink", model, "EDIT_FRIENDLINK", "编辑友情链接", cancellationToken);

    // ─── 动态 ───

    public Task<SdkResult<bool>> EditGameNewsAsync(EditGameNewsModel model, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/news/EditGameNews", model, "EDIT_GAME_NEWS", "编辑动态", cancellationToken);

    public Task<SdkResult<bool>> AddCustomNewsAsync(EditGameNewsModel model, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/news/AddCustomNews", model, "ADD_CUSTOM_NEWS", "添加自定义动态", cancellationToken);

    public Task<SdkResult<bool>> PublishGameNewsAsync(long id, CancellationToken cancellationToken = default)
        => GetCommandAsync($"api/news/PublishGameNews/{id}", "PUBLISH_GAME_NEWS", "发布动态", cancellationToken);

    public Task<SdkResult<bool>> IgnoreGameNewsAsync(long[] ids, bool isIgnore, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/news/IgnoreGameNews",
            new IgnoreGameNewsModel { Ids = ids, IsIgnore = isIgnore },
            "IGNORE_GAME_NEWS", "忽略/恢复动态", cancellationToken);

    public Task<SdkResult<bool>> AddWeiboNewsAsync(string link, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/news/AddWeiboNews",
            new AddWeiboNewsModel { Link = link },
            "ADD_WEIBO_NEWS", "导入微博动态", cancellationToken);

    // ─── 周报 ───

    public Task<SdkResult<bool>> EditWeeklyNewsAsync(EditWeeklyNewsModel model, CancellationToken cancellationToken = default)
        => PostCommandAsync("api/news/EditWeeklyNews", model, "EDIT_WEEKLY_NEWS", "编辑周报", cancellationToken);

    public Task<SdkResult<bool>> PublishWeeklyNewsAsync(long id, CancellationToken cancellationToken = default)
        => GetCommandAsync($"api/news/PublishWeelyNews/{id}", "PUBLISH_WEEKLY_NEWS", "发布周报", cancellationToken);

    public Task<SdkResult<bool>> ResetWeeklyNewsAsync(long id, CancellationToken cancellationToken = default)
        => GetCommandAsync($"api/news/ResetWeelyNews/{id}", "RESET_WEEKLY_NEWS", "重置周报", cancellationToken);
}
