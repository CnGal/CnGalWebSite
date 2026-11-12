using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.DataModel.ViewModel;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class ExamineQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<ExamineQueryService> logger,
    ExamineUserCache userCache,
    ICurrentUserAccessor currentUserAccessor)
    : QueryServiceBase(httpClient), IExamineQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    protected override ILogger Logger => logger;

    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(2);

    private Task<SdkResult<QueryResultModel<T, TSort>>> QueryListAsync<T, TSort>(
        string apiPath,
        string errorDomain,
        string displayName,
        QueryParameterModel<TSort> parameter,
        CancellationToken cancellationToken) where T : class where TSort : struct, Enum
    {
        // 固定请求参数，避免等待身份期间调用方修改参数，导致键与请求体不一致。
        var query = new QueryParameterModel<TSort>
        {
            Page = parameter.Page,
            ItemsPerPage = parameter.ItemsPerPage,
            SearchText = parameter.SearchText,
            Sorts = parameter.Sorts?.ToArray()!
        };
        var queryHash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(query, SdkJsonSerializerOptions.Default)));
        var requestKey = ExamineCacheKeys.List<TSort>(apiPath, queryHash);
        return WithIdentityAsync((scope, token) => userCache.GetOrCreateAsync(
            HttpClient.BaseAddress, scope, requestKey,
            loadToken => LoadListAsync<T, TSort>(apiPath, errorDomain, displayName, query, loadToken), token,
            apiPath == "api/examines/ListExamines" ? userCache.GetReviewQueueGeneration(HttpClient.BaseAddress) : null), cancellationToken);
    }

    private async Task<SdkResult<QueryResultModel<T, TSort>>> LoadListAsync<T, TSort>(
        string apiPath,
        string errorDomain,
        string displayName,
        QueryParameterModel<TSort> parameter,
        CancellationToken cancellationToken) where T : class where TSort : struct, Enum
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(apiPath, parameter, SdkJsonSerializerOptions.Default, cancellationToken);

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
                cancellationToken.ThrowIfCancellationRequested();
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 调用方取消直接传播；HTTP 超时仍走既有失败结果。
            throw;
        }
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

    // ─── 审核列表 ───

    public Task<SdkResult<QueryResultModel<ExamineOverviewModel, ExamineSortField>>> GetExaminesAsync(
        QueryParameterModel<ExamineSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExamineOverviewModel, ExamineSortField>("api/examines/ListExamines", "EXAMINE", "审核", parameter, cancellationToken);

    // ─── 审阅记录列表 ───

    public Task<SdkResult<QueryResultModel<UserReviewEditRecordOverviewModel, UserReviewEditRecordSortField>>> GetUserReviewEditRecordsAsync(
        QueryParameterModel<UserReviewEditRecordSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserReviewEditRecordOverviewModel, UserReviewEditRecordSortField>("api/examines/ListUserReviewEditRecords", "EXAMINE_REVIEW", "审阅记录", parameter, cancellationToken);

    // ─── 用户监控列表 ───

    public Task<SdkResult<QueryResultModel<UserMonitorOverviewModel, UserMonitorSortField>>> GetUserMonitorsAsync(
        QueryParameterModel<UserMonitorSortField> parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserMonitorOverviewModel, UserMonitorSortField>("api/examines/ListUserMonitors", "EXAMINE_MONITOR", "监控", parameter, cancellationToken);

    // ─── 用户内容中心概览 ───

    public Task<SdkResult<UserContentCenterViewModel>> GetUserContentCenterAsync(
        CancellationToken cancellationToken = default)
    {
        return WithIdentityAsync((scope, cancellation) => userCache.GetOrCreateAsync(
            HttpClient.BaseAddress,
            scope,
            ExamineCacheKeys.ContentCenter,
            token => GetAsync<UserContentCenterViewModel>(
                "api/examines/GetUserContentCenterView",
                "EXAMINE_CONTENT_CENTER",
                "用户内容中心",
                token),
            cancellation, userCache.GetReviewQueueGeneration(HttpClient.BaseAddress)), cancellationToken);
    }

    // ─── 编辑记录概览 ───

    public async Task<SdkResult<ExaminesOverviewViewModel>> GetExaminesOverviewAsync(
        long examineId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.Overview(examineId);
        if (memoryCache.TryGetValue(cacheKey, out ExaminesOverviewViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SdkResult<ExaminesOverviewViewModel>.Ok(cached);
        }

        var result = await GetAsync<ExaminesOverviewViewModel>(
            $"api/examines/GetExaminesOverview/{examineId}",
            "EXAMINE_OVERVIEW",
            "编辑记录概览",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, ListCacheDuration, cacheRead);
        }

        return result;
    }

    // ─── 单条审核详情 ───

    public Task<SdkResult<ExamineViewModel>> GetExamineDetailAsync(
        long examineId, CancellationToken cancellationToken = default)
        => WithIdentityAsync((scope, token) => GetExamineDetailAsync(examineId, scope, token), cancellationToken);

    public async Task<SdkResult<ExamineViewModel>> RefreshExamineDetailAsync(
        long examineId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        userCache.InvalidateExamine(HttpClient.BaseAddress, examineId);
        return await GetExamineDetailAsync(examineId, cancellationToken);
    }

    private Task<SdkResult<ExamineViewModel>> GetExamineDetailAsync(
        long examineId, CurrentUserCacheScope? scope, CancellationToken cancellationToken)
    {
        return userCache.GetOrCreateAsync(
            HttpClient.BaseAddress,
            scope,
            ExamineCacheKeys.Detail(examineId),
            token => GetAsync<ExamineViewModel>(
                $"api/examines/GetExamineView/{examineId}",
                "EXAMINE_DETAIL",
                "审核详情",
                token),
            cancellationToken, userCache.GetExamineGeneration(HttpClient.BaseAddress, examineId));
    }

    // Historical comparisons must replay records, never combine two live review-detail views.
    public async Task<SdkResult<EditRecordComparison>> GetEditRecordComparisonAsync(
        ExaminedNormalListModelType type, long contrastId, long currentId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (contrastId <= 0 || currentId <= 0 || contrastId > currentId)
            return SdkResult<EditRecordComparison>.Fail("EXAMINE_COMPARISON_RANGE", "请选择有效的版本，基准版本不能晚于目标版本");
        var path = type switch
        {
            ExaminedNormalListModelType.Entry => "entries", ExaminedNormalListModelType.Article => "articles",
            ExaminedNormalListModelType.Tag => "tags", ExaminedNormalListModelType.Periphery => "peripheries",
            ExaminedNormalListModelType.Video => "videos", _ => null
        };
        if (path is null) return SdkResult<EditRecordComparison>.Fail("EXAMINE_COMPARISON_TYPE", "该类型不支持历史版本对比");
        return await GetAsync<EditRecordComparison>($"api/{path}/GetContrastEditRecordViews/{contrastId}/{currentId}",
            "EXAMINE_COMPARISON", "历史版本对比", cancellationToken);
    }

    private async Task<SdkResult<T>> WithIdentityAsync<T>(
        Func<CurrentUserCacheScope?, CancellationToken, Task<SdkResult<T>>> query,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var scope = await currentUserAccessor.GetCacheScopeAsync(cancellationToken);
            using var linked = scope?.IdentityChanged.CanBeCanceled == true
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, scope.IdentityChanged)
                : null;
            try
            {
                scope?.IdentityChanged.ThrowIfCancellationRequested();
                var result = await query(scope, linked?.Token ?? cancellationToken);
                scope?.IdentityChanged.ThrowIfCancellationRequested();
                if (!result.Success) cancellationToken.ThrowIfCancellationRequested();
                return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && scope?.IdentityChanged.IsCancellationRequested == true)
            {
                // 身份或令牌变更后重新取得快照，整个查询（包括双版本对比）最多重试一次。
            }
        }
        return SdkResult<T>.Fail("EXAMINE_AUTH_CONTEXT_CHANGED", "登录状态已变更，请重新加载");
    }

    // ─── 词条编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetEntryEditRecordsAsync(
        int entryId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.EntryEditRecords(entryId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(cached);
        }

        var result = await GetAsync<EditEntryInforBindModel>(
            $"api/entries/GetEntryEditInforBindModel/{entryId}",
            "ENTRY_EDIT_RECORDS",
            "词条编辑记录",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Fail(
                result.Error?.Code ?? "ENTRY_EDIT_RECORDS_FAILED",
                result.Error?.Message ?? "获取词条编辑记录失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<ExaminedNormalListModel> records = result.Data.Examines
            .Where(e => e.IsPassed == true)
            .OrderByDescending(e => e.ApplyTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, records, ListCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 文章编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetArticleEditRecordsAsync(
        long articleId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.ArticleEditRecords(articleId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(cached);
        }

        var result = await GetAsync<EditArticleInforBindModel>(
            $"api/articles/GetArticleEditInforBindModel/{articleId}",
            "ARTICLE_EDIT_RECORDS",
            "文章编辑记录",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Fail(
                result.Error?.Code ?? "ARTICLE_EDIT_RECORDS_FAILED",
                result.Error?.Message ?? "获取文章编辑记录失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<ExaminedNormalListModel> records = result.Data.Examines
            .Where(e => e.IsPassed == true)
            .OrderByDescending(e => e.ApplyTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, records, ListCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 视频编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetVideoEditRecordsAsync(
        long videoId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.VideoEditRecords(videoId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(cached);
        }

        var result = await GetAsync<EditVideoInforBindModel>(
            $"api/videos/GetEditInforBindModel/{videoId}",
            "VIDEO_EDIT_RECORDS",
            "视频编辑记录",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Fail(
                result.Error?.Code ?? "VIDEO_EDIT_RECORDS_FAILED",
                result.Error?.Message ?? "获取视频编辑记录失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<ExaminedNormalListModel> records = result.Data.Examines
            .Where(e => e.IsPassed == true)
            .OrderByDescending(e => e.ApplyTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, records, ListCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 标签编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetTagEditRecordsAsync(
        long tagId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.TagEditRecords(tagId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(cached);
        }

        var result = await GetAsync<EditTagInforBindModel>(
            $"api/tags/GetTagEditInforBindModel/{tagId}",
            "TAG_EDIT_RECORDS",
            "标签编辑记录",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Fail(
                result.Error?.Code ?? "TAG_EDIT_RECORDS_FAILED",
                result.Error?.Message ?? "获取标签编辑记录失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<ExaminedNormalListModel> records = result.Data.Examines
            .Where(e => e.IsPassed == true)
            .OrderByDescending(e => e.ApplyTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, records, ListCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 周边编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetPeripheryEditRecordsAsync(
        long peripheryId, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.PeripheryEditRecords(peripheryId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(cached);
        }

        var result = await GetAsync<EditPeripheryInforBindModel>(
            $"api/Peripheries/GetPeripheryEditInforBindModel/{peripheryId}",
            "PERIPHERY_EDIT_RECORDS",
            "周边编辑记录",
            cancellationToken);

        if (!result.Success || result.Data is null)
        {
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Fail(
                result.Error?.Code ?? "PERIPHERY_EDIT_RECORDS_FAILED",
                result.Error?.Message ?? "获取周边编辑记录失败",
                result.Error?.StatusCode);
        }

        IReadOnlyList<ExaminedNormalListModel> records = result.Data.Examines
            .Where(e => e.IsPassed == true)
            .OrderByDescending(e => e.ApplyTime)
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(cacheKey, records, ListCacheDuration, cacheRead);
        cancellationToken.ThrowIfCancellationRequested();
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }
}
