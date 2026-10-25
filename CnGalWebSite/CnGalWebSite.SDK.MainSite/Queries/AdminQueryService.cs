using CnGalWebSite.DataModel.Sorting;
using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Almanacs;
using CnGalWebSite.DataModel.ViewModel.BackUpArchives;
using CnGalWebSite.DataModel.ViewModel.Commodities;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.DataModel.ViewModel.News;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.PlayedGames;
using CnGalWebSite.DataModel.ViewModel.Questionnaires;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Recommends;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Extensions;

using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class AdminQueryService(HttpClient httpClient, ILogger<AdminQueryService> logger)
    : QueryServiceBase(httpClient), IAdminQueryService
{
    protected override ILogger Logger => logger;

    public Task<SdkResult<ServerStaticOverviewModel>> GetServerOverviewAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<ServerStaticOverviewModel>(
            "api/admin/GetServerStaticDataOverview",
            "ADMIN",
            "服务器概览",
            cancellationToken);
    }

    public Task<SdkResult<ServerRealTimeOverviewModel>> GetServerRealTimeOverviewAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<ServerRealTimeOverviewModel>(
            "api/admin/GetServerRealTimeDataOverview",
            "ADMIN",
            "服务器动态数据概览",
            cancellationToken);
    }

    public Task<SdkResult<UserDataOverviewModel>> GetUserDataOverviewAsync(CancellationToken cancellationToken = default)
    {
        return GetAsync<UserDataOverviewModel>(
            "api/admin/GetUserDataOverview",
            "ADMIN",
            "用户概览",
            cancellationToken);
    }

    // ─── 通用分页查询辅助方法 ───

    private async Task<SdkResult<QueryResultModel<T, TSort>>> QueryListAsync<T, TSort>(
        string apiPath, string errorDomain, string displayName,
        QueryParameterModel<TSort> parameter, CancellationToken cancellationToken) where T : class where TSort : struct, Enum
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(
                apiPath, parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "{DisplayName}列表接口请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    displayName, apiPath, (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<T, TSort>>.Fail(
                    $"{errorDomain}_HTTP_FAILED",
                    $"获取{displayName}列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<T, TSort>>(body);
                return SdkResult<QueryResultModel<T, TSort>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "{DisplayName}列表反序列化失败。Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    displayName, apiPath, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<T, TSort>>.Fail(
                    $"{errorDomain}_DESERIALIZE_FAILED",
                    $"{displayName}列表数据格式不兼容");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取{DisplayName}列表异常。Path={Path}; BaseAddress={BaseAddress}",
                displayName, apiPath, HttpClient.BaseAddress);
            return SdkFailure.FromException<QueryResultModel<T, TSort>>(ex,
                $"{errorDomain}_EXCEPTION",
                $"请求{displayName}列表时发生异常");
        }
    }

    // ─── 各领域分页查询 ───

    public Task<SdkResult<QueryResultModel<CommentOverviewModel, CommentSortField>>> GetCommentsAsync(
        QueryParameterModel<CommentSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<CommentOverviewModel, CommentSortField>("api/comments/List", "ADMIN_COMMENTS", "评论", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<EntryOverviewModel, EntrySortField>>> GetEntriesAsync(
        QueryParameterModel<EntrySortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<EntryOverviewModel, EntrySortField>("api/entries/List", "ADMIN_ENTRIES", "词条", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<ArticleOverviewModel, ArticleSortField>>> GetArticlesAsync(
        QueryParameterModel<ArticleSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ArticleOverviewModel, ArticleSortField>("api/articles/List", "ADMIN_ARTICLES", "文章", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<VideoOverviewModel, VideoSortField>>> GetVideosAsync(
        QueryParameterModel<VideoSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<VideoOverviewModel, VideoSortField>("api/videos/List", "ADMIN_VIDEOS", "视频", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<TagOverviewModel, TagSortField>>> GetTagsAsync(
        QueryParameterModel<TagSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<TagOverviewModel, TagSortField>("api/tags/List", "ADMIN_TAGS", "标签", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<UserOverviewModel, UserSortField>>> GetUsersAsync(
        QueryParameterModel<UserSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserOverviewModel, UserSortField>("api/account/ListUsers", "ADMIN_USERS", "用户", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<MessageOverviewModel, MessageSortField>>> GetMessagesAsync(
        QueryParameterModel<MessageSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<MessageOverviewModel, MessageSortField>("api/space/ListMessages", "ADMIN_MESSAGES", "消息", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<FavoriteFolderOverviewModel, FavoriteFolderSortField>>> GetFavoriteFoldersAsync(
        QueryParameterModel<FavoriteFolderSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<FavoriteFolderOverviewModel, FavoriteFolderSortField>("api/Favorites/ListFavoriteFolders", "ADMIN_FAVORITES", "收藏夹", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<BackUpArchiveOverviewModel, BackUpArchiveSortField>>> GetBackUpArchivesAsync(
        QueryParameterModel<BackUpArchiveSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<BackUpArchiveOverviewModel, BackUpArchiveSortField>("api/backuparchives/list", "ADMIN_BACKUPS", "备份", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<StoreInfoOverviewModel, StoreInfoSortField>>> GetStoreInfoListAsync(
        QueryParameterModel<StoreInfoSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<StoreInfoOverviewModel, StoreInfoSortField>("api/storeinfo/list", "ADMIN_STOREINFO", "商店信息", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<RecommendOverviewModel, RecommendSortField>>> GetRecommendsAsync(
        QueryParameterModel<RecommendSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<RecommendOverviewModel, RecommendSortField>("api/recommends/list", "ADMIN_RECOMMENDS", "推荐", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<AlmanacOverviewModel, AlmanacSortField>>> GetAlmanacsAsync(
        QueryParameterModel<AlmanacSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<AlmanacOverviewModel, AlmanacSortField>("api/almanac/List", "ADMIN_ALMANACS", "年鉴", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<LotteryOverviewModel, LotterySortField>>> GetLotteriesAsync(
        QueryParameterModel<LotterySortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<LotteryOverviewModel, LotterySortField>("api/lotteries/List", "ADMIN_LOTTERIES", "抽奖", parameter, cancellationToken);

    public async Task<SdkResult<QueryResultModel<LotteryUserOverviewModel, LotteryUserSortField>>> GetLotteryUsersAsync(
        long lotteryId, QueryParameterModel<LotteryUserSortField> parameter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(
                $"api/lotteries/ListLotteryUsers?lotteryId={lotteryId}",
                parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "抽奖参与用户列表请求失败。LotteryId={LotteryId}; StatusCode={StatusCode}; ResponseBody={ResponseBody}",
                    lotteryId, (int)response.StatusCode, TrimForLog(body));
                return SdkResult<QueryResultModel<LotteryUserOverviewModel, LotteryUserSortField>>.Fail(
                    "ADMIN_LOTTERY_USERS_HTTP_FAILED",
                    $"获取参与用户列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<LotteryUserOverviewModel, LotteryUserSortField>>(body);
                return SdkResult<QueryResultModel<LotteryUserOverviewModel, LotteryUserSortField>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "抽奖参与用户列表反序列化失败。LotteryId={LotteryId}; ResponseBody={ResponseBody}",
                    lotteryId, TrimForLog(body));
                return SdkResult<QueryResultModel<LotteryUserOverviewModel, LotteryUserSortField>>.Fail(
                    "ADMIN_LOTTERY_USERS_DESERIALIZE_FAILED", "参与用户列表数据格式不兼容");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex, "获取抽奖参与用户列表异常。LotteryId={LotteryId}", lotteryId);
            return SdkFailure.FromException<QueryResultModel<LotteryUserOverviewModel, LotteryUserSortField>>(ex,
                "ADMIN_LOTTERY_USERS_EXCEPTION", "请求参与用户列表时发生异常");
        }
    }

    public Task<SdkResult<List<WinnerOverviewModel>>> GetLotteryWinnersAsync(
        long lotteryId, CancellationToken cancellationToken = default)
        => GetAsync<List<WinnerOverviewModel>>(
            $"api/lotteries/GetWinnerDatas/{lotteryId}",
            "ADMIN_LOTTERY_WINNERS", "中奖用户列表", cancellationToken);

    public Task<SdkResult<QueryResultModel<PeripheryOverviewModel, PeripherySortField>>> GetPeripheriesAsync(
        QueryParameterModel<PeripherySortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<PeripheryOverviewModel, PeripherySortField>("api/peripheries/List", "ADMIN_PERIPHERIES", "周边", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<VoteOverviewModel, VoteSortField>>> GetVotesAsync(
        QueryParameterModel<VoteSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<VoteOverviewModel, VoteSortField>("api/votes/List", "ADMIN_VOTES", "投票", parameter, cancellationToken);

    // ─── 网站设置 ───

    public Task<SdkResult<QueryResultModel<CarouselOverviewModel, CarouselSortField>>> GetCarouselsAsync(
        QueryParameterModel<CarouselSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<CarouselOverviewModel, CarouselSortField>("api/home/ListCarousels", "ADMIN_CAROUSELS", "轮播图", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<FriendLinkOverviewModel, FriendLinkSortField>>> GetFriendLinksAsync(
        QueryParameterModel<FriendLinkSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<FriendLinkOverviewModel, FriendLinkSortField>("api/home/ListFriendLinks", "ADMIN_FRIENDLINKS", "友情链接", parameter, cancellationToken);

    public Task<SdkResult<CarouselEditModel>> GetCarouselEditAsync(
        int id, CancellationToken cancellationToken = default)
        => GetSingleAsync<CarouselEditModel, CarouselEditModel>(
            $"api/home/EditCarousel?id={id}", dto => dto,
            "ADMIN_CAROUSEL", "轮播图", id, cancellationToken);

    public Task<SdkResult<FriendLinkEditModel>> GetFriendLinkEditAsync(
        int id, CancellationToken cancellationToken = default)
        => GetSingleAsync<FriendLinkEditModel, FriendLinkEditModel>(
            $"api/home/EditFriendLink?id={id}", dto => dto,
            "ADMIN_FRIENDLINK", "友情链接", id, cancellationToken);

    // ─── 动态/周报 ───

    public Task<SdkResult<QueryResultModel<GameNewsOverviewModel, GameNewsSortField>>> GetGameNewsAsync(
        QueryParameterModel<GameNewsSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<GameNewsOverviewModel, GameNewsSortField>("api/news/ListGameNews", "ADMIN_GAME_NEWS", "游戏动态", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<WeeklyNewsOverviewModel, WeeklyNewsSortField>>> GetWeeklyNewsAsync(
        QueryParameterModel<WeeklyNewsSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<WeeklyNewsOverviewModel, WeeklyNewsSortField>("api/news/ListWeeklyNews", "ADMIN_WEEKLY_NEWS", "周报", parameter, cancellationToken);

    public Task<SdkResult<EditGameNewsModel>> GetGameNewsEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<EditGameNewsModel, EditGameNewsModel>(
            $"api/news/EditGameNews/{id}", dto => dto,
            "ADMIN_GAME_NEWS", "动态", id, cancellationToken);

    public Task<SdkResult<EditWeeklyNewsModel>> GetWeeklyNewsEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<EditWeeklyNewsModel, EditWeeklyNewsModel>(
            $"api/news/EditWeeklyNews/{id}", dto => dto,
            "ADMIN_WEEKLY_NEWS", "周报", id, cancellationToken);

    public Task<SdkResult<ArticleViewModel>> GetGameNewsPreviewAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ArticleViewModel, ArticleViewModel>(
            $"api/news/GetGameNewsPreview/{id}", dto => dto,
            "ADMIN_GAME_NEWS_PREVIEW", "动态预览", id, cancellationToken);

    public Task<SdkResult<ArticleViewModel>> GetWeeklyNewsPreviewAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ArticleViewModel, ArticleViewModel>(
            $"api/news/GetWeelyNewsPreview/{id}", dto => dto,
            "ADMIN_WEEKLY_NEWS_PREVIEW", "周报预览", id, cancellationToken);

    // ─── 展会管理 ───

    public Task<SdkResult<QueryResultModel<ExpoActivityOverviewModel, ExpoActivitySortField>>> GetExpoActivitiesAsync(
        QueryParameterModel<ExpoActivitySortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoActivityOverviewModel, ExpoActivitySortField>("api/expo/ListActivity", "ADMIN_EXPO_ACTIVITIES", "展会活动", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<ExpoTicketOverviewModel, ExpoTicketSortField>>> GetExpoTicketsAsync(
        QueryParameterModel<ExpoTicketSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoTicketOverviewModel, ExpoTicketSortField>("api/expo/ListTicket", "ADMIN_EXPO_TICKETS", "展会票根", parameter, cancellationToken);

    // ─── 小卖铺 ───

    public Task<SdkResult<QueryResultModel<CommodityOverviewModel, CommoditySortField>>> GetCommoditiesAsync(
        QueryParameterModel<CommoditySortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<CommodityOverviewModel, CommoditySortField>("api/commodities/List", "ADMIN_COMMODITIES", "商品", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<CommodityCodeOverviewModel, CommodityCodeSortField>>> GetCommodityCodesAsync(
        QueryParameterModel<CommodityCodeSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<CommodityCodeOverviewModel, CommodityCodeSortField>("api/commodities/ListCode", "ADMIN_COMMODITY_CODES", "兑换码", parameter, cancellationToken);

    // ─── 展会编辑模型查询（P2 CRUD） ───

    public Task<SdkResult<ExpoActivityEditModel>> GetExpoActivityEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ExpoActivityEditModel, ExpoActivityEditModel>(
            $"api/expo/EditActivity?id={id}", dto => dto,
            "ADMIN_EXPO_ACTIVITY", "展会活动", id, cancellationToken);

    public Task<SdkResult<ExpoTicketEditModel>> GetExpoTicketEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ExpoTicketEditModel, ExpoTicketEditModel>(
            $"api/expo/EditTicket?id={id}", dto => dto,
            "ADMIN_EXPO_TICKET", "展会票根", id, cancellationToken);

    // ─── 展会扩展管理 ───

    public Task<SdkResult<QueryResultModel<ExpoGameOverviewModel, ExpoGameSortField>>> GetExpoGamesAsync(
        QueryParameterModel<ExpoGameSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoGameOverviewModel, ExpoGameSortField>("api/expo/ListGame", "ADMIN_EXPO_GAMES", "展会游戏", parameter, cancellationToken);

    public Task<SdkResult<ExpoGameEditModel>> GetExpoGameEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ExpoGameEditModel, ExpoGameEditModel>(
            $"api/expo/EditGame?id={id}", dto => dto,
            "ADMIN_EXPO_GAME", "展会游戏", id, cancellationToken);

    public Task<SdkResult<QueryResultModel<ExpoTagOverviewModel, ExpoTagSortField>>> GetExpoTagsExpoAsync(
        QueryParameterModel<ExpoTagSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoTagOverviewModel, ExpoTagSortField>("api/expo/ListTag", "ADMIN_EXPO_TAGS", "展会标签", parameter, cancellationToken);

    public Task<SdkResult<ExpoTagEditModel>> GetExpoTagEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ExpoTagEditModel, ExpoTagEditModel>(
            $"api/expo/EditTag?id={id}", dto => dto,
            "ADMIN_EXPO_TAG", "展会标签", id, cancellationToken);

    public Task<SdkResult<QueryResultModel<ExpoTaskOverviewModel, ExpoTaskSortField>>> GetExpoTasksAsync(
        QueryParameterModel<ExpoTaskSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoTaskOverviewModel, ExpoTaskSortField>("api/expo/ListTask", "ADMIN_EXPO_TASKS", "展会任务", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<ExpoAwardOverviewModel, ExpoAwardSortField>>> GetExpoAwardsAsync(
        QueryParameterModel<ExpoAwardSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoAwardOverviewModel, ExpoAwardSortField>("api/expo/ListAward", "ADMIN_EXPO_AWARDS", "展会奖项", parameter, cancellationToken);

    public Task<SdkResult<ExpoAwardEditModel>> GetExpoAwardEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<ExpoAwardEditModel, ExpoAwardEditModel>(
            $"api/expo/EditAward?id={id}", dto => dto,
            "ADMIN_EXPO_AWARD", "展会奖项", id, cancellationToken);

    public Task<SdkResult<QueryResultModel<ExpoPrizeOverviewModel, ExpoPrizeSortField>>> GetExpoPrizesAsync(
        QueryParameterModel<ExpoPrizeSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExpoPrizeOverviewModel, ExpoPrizeSortField>("api/expo/ListPrize", "ADMIN_EXPO_PRIZES", "展会奖品", parameter, cancellationToken);

    // ─── 问卷管理 ───

    public Task<SdkResult<QueryResultModel<QuestionnaireOverviewModel, QuestionnaireSortField>>> GetQuestionnairesAsync(
        QueryParameterModel<QuestionnaireSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<QuestionnaireOverviewModel, QuestionnaireSortField>("api/questionnaires/List", "ADMIN_QUESTIONNAIRES", "问卷", parameter, cancellationToken);

    public Task<SdkResult<EditQuestionnaireModel>> GetQuestionnaireEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<EditQuestionnaireModel, EditQuestionnaireModel>(
            $"api/questionnaires/EditQuestionnaire/{id}", dto => dto,
            "ADMIN_QUESTIONNAIRE", "问卷", id, cancellationToken);

    public Task<SdkResult<QuestionnaireStatisticsModel>> GetQuestionnaireStatisticsAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<QuestionnaireStatisticsModel, QuestionnaireStatisticsModel>(
            $"api/questionnaires/GetQuestionnaireStatistics/{id}", dto => dto,
            "ADMIN_QUESTIONNAIRE_STATS", "问卷统计", id, cancellationToken);

    public Task<SdkResult<UserResponseListModel>> GetUserResponseListAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<UserResponseListModel, UserResponseListModel>(
            $"api/questionnaires/GetUserResponseList?id={id}", dto => dto,
            "ADMIN_QUESTIONNAIRE_RESPONSES", "问卷回答列表", id, cancellationToken);

    public Task<SdkResult<UserResponseDetailModel>> GetUserResponseDetailAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<UserResponseDetailModel, UserResponseDetailModel>(
            $"api/questionnaires/GetUserResponseDetail?id={id}", dto => dto,
            "ADMIN_QUESTIONNAIRE_RESPONSE_DETAIL", "回答详情", id, cancellationToken);

    public Task<SdkResult<QuestionAnalysisModel>> GetQuestionAnalysisAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<QuestionAnalysisModel, QuestionAnalysisModel>(
            $"api/questionnaires/GetQuestionAnalysis?id={id}", dto => dto,
            "ADMIN_QUESTIONNAIRE_ANALYSIS", "题目分析", id, cancellationToken);

    // ─── 小卖铺编辑模型查询（P2 CRUD） ───

    public Task<SdkResult<CommodityEditModel>> GetCommodityEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<CommodityEditModel, CommodityEditModel>(
            $"api/commodities/Edit?id={id}", dto => dto,
            "ADMIN_COMMODITY", "商品", id, cancellationToken);

    public Task<SdkResult<CommodityCodeEditModel>> GetCommodityCodeEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<CommodityCodeEditModel, CommodityCodeEditModel>(
            $"api/commodities/EditCode?id={id}", dto => dto,
            "ADMIN_COMMODITY_CODE", "兑换码", id, cancellationToken);

    // ─── 词条子功能 ───

    public Task<SdkResult<QueryResultModel<GameRecordOverviewModel, GameRecordSortField>>> GetPlayedGamesAsync(
        QueryParameterModel<GameRecordSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<GameRecordOverviewModel, GameRecordSortField>("api/playedgame/List", "ADMIN_PLAYED_GAMES", "游玩记录", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<EntryInformationTypeOverviewModel, EntryInformationTypeSortField>>> GetEntryInformationTypesAsync(
        QueryParameterModel<EntryInformationTypeSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<EntryInformationTypeOverviewModel, EntryInformationTypeSortField>("api/entries/ListEntryInformationTypes", "ADMIN_ENTRY_INFO_TYPES", "基础信息类型", parameter, cancellationToken);

    // ─── 用户子功能 ───

    public Task<SdkResult<QueryResultModel<RankOverviewModel, RankSortField>>> GetRanksAsync(
        QueryParameterModel<RankSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<RankOverviewModel, RankSortField>("api/ranks/ListRanks", "ADMIN_RANKS", "头衔", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<UserCertificationOverviewModel, UserCertificationSortField>>> GetUserCertificationsAsync(
        QueryParameterModel<UserCertificationSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserCertificationOverviewModel, UserCertificationSortField>("api/account/ListUserCertifications", "ADMIN_CERTIFICATIONS", "认证", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<OperationRecordOverviewModel, OperationRecordSortField>>> GetOperationRecordsAsync(
        QueryParameterModel<OperationRecordSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<OperationRecordOverviewModel, OperationRecordSortField>("api/account/ListOperationRecords", "ADMIN_OP_RECORDS", "操作记录", parameter, cancellationToken);

    // ─── 数据图表 ───

    public async Task<SdkResult<LineChartModel>> GetLineChartAsync(
        LineChartType type, DateTime afterTime, DateTime beforeTime,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var afterMs = afterTime.ToUnixTimeMilliseconds();
        var beforeMs = beforeTime.ToUnixTimeMilliseconds();
        return await GetAsync<LineChartModel>(
            $"api/admin/GetLineChart?Type={type}&AfterTime={afterMs}&BeforeTime={beforeMs}",
            "ADMIN_CHART",
            "图表数据",
            cancellationToken);
    }

    // ─── 编辑模型查询（P0 CRUD） ───

    public Task<SdkResult<RecommendEditModel>> GetRecommendEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<RecommendEditModel, RecommendEditModel>(
            $"api/recommends/Edit?id={id}", dto => dto,
            "ADMIN_RECOMMEND", "推荐", id, cancellationToken);

    public Task<SdkResult<StoreInfoEditModel>> GetStoreInfoEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<StoreInfoEditModel, StoreInfoEditModel>(
            $"api/storeinfo/Edit?id={id}", dto => dto,
            "ADMIN_STOREINFO", "商店信息", id, cancellationToken);

    public Task<SdkResult<EntryInformationTypeEditModel>> GetEntryInformationTypeEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<EntryInformationTypeEditModel, EntryInformationTypeEditModel>(
            $"api/entries/EditEntryInformationType?id={id}", dto => dto,
            "ADMIN_ENTRY_INFO_TYPE", "基础信息类型", id, cancellationToken);

    public Task<SdkResult<RankEditModel>> GetRankEditAsync(
        long id, CancellationToken cancellationToken = default)
        => GetSingleAsync<RankEditModel, RankEditModel>(
            $"api/ranks/Edit?id={id}", dto => dto,
            "ADMIN_RANK", "头衔", id, cancellationToken);
}
