using CnGalWebSite.DataModel.Sorting;
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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class ExamineQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<ExamineQueryService> logger,
    ExamineUserCache userCache,
    ICurrentUserAccessor currentUserAccessor)
    : QueryServiceBase(httpClient), IExamineQueryService
{
    protected override ILogger Logger => logger;
    protected override bool PropagateCancellation => true;

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
            return SdkResult<QueryResultModel<T, TSort>>.Fail(
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
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.Overview(examineId);
        if (memoryCache.TryGetValue(cacheKey, out ExaminesOverviewViewModel? cached) && cached is not null)
        {
            return SdkResult<ExaminesOverviewViewModel>.Ok(cached);
        }

        var result = await GetAsync<ExaminesOverviewViewModel>(
            $"api/examines/GetExaminesOverview/{examineId}",
            "EXAMINE_OVERVIEW",
            "编辑记录概览",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, ListCacheDuration);
        }

        return result;
    }

    // ─── 单条审核详情 ───

    public Task<SdkResult<ExamineViewModel>> GetExamineDetailAsync(
        long examineId, CancellationToken cancellationToken = default)
        => WithIdentityAsync((scope, token) => GetExamineDetailAsync(examineId, scope, token), cancellationToken);

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

    // ─── 对比审核（两个版本的 Before/After） ───

    public Task<SdkResult<ExamineViewModel>> GetContrastExamineAsync(
        long contrastId, long currentId, CancellationToken cancellationToken = default)
        => WithIdentityAsync((scope, token) => GetContrastExamineAsync(contrastId, currentId, scope, token), cancellationToken);

    private async Task<SdkResult<ExamineViewModel>> GetContrastExamineAsync(
        long contrastId, long currentId, CurrentUserCacheScope? scope, CancellationToken cancellationToken)
    {
        // 分别加载两条审核记录的详情
        // BeforeModel 取自 contrastId 的 AfterModel（对比版本的终态）
        // AfterModel 取自 currentId 的 AfterModel（当前版本的终态）
        var contrastResult = await GetExamineDetailAsync(contrastId, scope, cancellationToken);
        if (!contrastResult.Success || contrastResult.Data is null)
        {
            return contrastResult;
        }

        var currentResult = await GetExamineDetailAsync(currentId, scope, cancellationToken);
        if (!currentResult.Success || currentResult.Data is null)
        {
            return currentResult;
        }

        // 组合成对比模型：Before = contrast 的 AfterModel, After = current 的 AfterModel
        var combined = new ExamineViewModel
        {
            Id = contrastResult.Data.Id,
            ObjectId = contrastResult.Data.ObjectId,
            ObjectName = contrastResult.Data.ObjectName,
            ObjectBriefIntroduction = contrastResult.Data.ObjectBriefIntroduction,
            Image = contrastResult.Data.Image,
            IsThumbnail = contrastResult.Data.IsThumbnail,
            Type = contrastResult.Data.Type,
            Operation = contrastResult.Data.Operation,
            EditOverview = contrastResult.Data.EditOverview,
            IsPassed = contrastResult.Data.IsPassed,
            PassedAdminName = contrastResult.Data.PassedAdminName,
            IsAdmin = contrastResult.Data.IsAdmin,
            Comments = contrastResult.Data.Comments,
            Note = contrastResult.Data.Note,
            UserId = contrastResult.Data.UserId,
            UserName = contrastResult.Data.UserName,
            ApplyTime = contrastResult.Data.ApplyTime,
            PassedTime = contrastResult.Data.PassedTime,
            PrepositionExamineId = contrastResult.Data.PrepositionExamineId,
            ContributionValue = contrastResult.Data.ContributionValue,
            SensitiveWords = contrastResult.Data.SensitiveWords,
            BeforeModel = contrastResult.Data.AfterModel,
            AfterModel = currentResult.Data.AfterModel,
        };

        return SdkResult<ExamineViewModel>.Ok(combined);
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
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.EntryEditRecords(entryId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached) && cached is not null)
        {
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

        memoryCache.Set(cacheKey, records, ListCacheDuration);
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 文章编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetArticleEditRecordsAsync(
        long articleId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.ArticleEditRecords(articleId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached) && cached is not null)
        {
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

        memoryCache.Set(cacheKey, records, ListCacheDuration);
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 视频编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetVideoEditRecordsAsync(
        long videoId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.VideoEditRecords(videoId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached) && cached is not null)
        {
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

        memoryCache.Set(cacheKey, records, ListCacheDuration);
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 标签编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetTagEditRecordsAsync(
        long tagId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.TagEditRecords(tagId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached) && cached is not null)
        {
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

        memoryCache.Set(cacheKey, records, ListCacheDuration);
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }

    // ─── 周边编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetPeripheryEditRecordsAsync(
        long peripheryId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = ExamineCacheKeys.PeripheryEditRecords(peripheryId);
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached) && cached is not null)
        {
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

        memoryCache.Set(cacheKey, records, ListCacheDuration);
        return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(records);
    }
}
