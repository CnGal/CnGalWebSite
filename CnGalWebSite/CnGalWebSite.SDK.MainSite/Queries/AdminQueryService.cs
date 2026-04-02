using System.Net.Http.Json;
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

    // ─── 通用分页查询辅助方法 ───

    private async Task<SdkResult<QueryResultModel<T>>> QueryListAsync<T>(
        string apiPath, string errorDomain, string displayName,
        QueryParameterModel parameter, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                apiPath, parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "{DisplayName}列表接口请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    displayName, apiPath, (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<T>>.Fail(
                    $"{errorDomain}_HTTP_FAILED",
                    $"获取{displayName}列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<T>>(body);
                return SdkResult<QueryResultModel<T>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "{DisplayName}列表反序列化失败。Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    displayName, apiPath, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<T>>.Fail(
                    $"{errorDomain}_DESERIALIZE_FAILED",
                    $"{displayName}列表数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取{DisplayName}列表异常。Path={Path}; BaseAddress={BaseAddress}",
                displayName, apiPath, HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<T>>.Fail(
                $"{errorDomain}_EXCEPTION",
                $"请求{displayName}列表时发生异常");
        }
    }

    // ─── 各领域分页查询 ───

    public Task<SdkResult<QueryResultModel<CommentOverviewModel>>> GetCommentsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<CommentOverviewModel>("api/comments/List", "ADMIN_COMMENTS", "评论", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<EntryOverviewModel>>> GetEntriesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<EntryOverviewModel>("api/entries/List", "ADMIN_ENTRIES", "词条", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<ArticleOverviewModel>>> GetArticlesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ArticleOverviewModel>("api/articles/List", "ADMIN_ARTICLES", "文章", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<VideoOverviewModel>>> GetVideosAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<VideoOverviewModel>("api/videos/List", "ADMIN_VIDEOS", "视频", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<TagOverviewModel>>> GetTagsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<TagOverviewModel>("api/tags/List", "ADMIN_TAGS", "标签", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<UserOverviewModel>>> GetUsersAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserOverviewModel>("api/account/ListUsers", "ADMIN_USERS", "用户", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<MessageOverviewModel>>> GetMessagesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<MessageOverviewModel>("api/space/ListMessages", "ADMIN_MESSAGES", "消息", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<FavoriteFolderOverviewModel>>> GetFavoriteFoldersAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<FavoriteFolderOverviewModel>("api/Favorites/ListFavoriteFolders", "ADMIN_FAVORITES", "收藏夹", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<BackUpArchiveOverviewModel>>> GetBackUpArchivesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<BackUpArchiveOverviewModel>("api/backuparchives/list", "ADMIN_BACKUPS", "备份", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<StoreInfoOverviewModel>>> GetStoreInfoListAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<StoreInfoOverviewModel>("api/storeinfo/list", "ADMIN_STOREINFO", "商店信息", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<RecommendOverviewModel>>> GetRecommendsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<RecommendOverviewModel>("api/recommends/list", "ADMIN_RECOMMENDS", "推荐", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<AlmanacOverviewModel>>> GetAlmanacsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<AlmanacOverviewModel>("api/almanac/List", "ADMIN_ALMANACS", "年鉴", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<LotteryOverviewModel>>> GetLotteriesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<LotteryOverviewModel>("api/lotteries/List", "ADMIN_LOTTERIES", "抽奖", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<PeripheryOverviewModel>>> GetPeripheriesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<PeripheryOverviewModel>("api/peripheries/List", "ADMIN_PERIPHERIES", "周边", parameter, cancellationToken);

    public Task<SdkResult<QueryResultModel<VoteOverviewModel>>> GetVotesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<VoteOverviewModel>("api/votes/List", "ADMIN_VOTES", "投票", parameter, cancellationToken);
}
