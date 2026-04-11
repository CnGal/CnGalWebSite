using System.Net.Http.Json;
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

public sealed class ExamineQueryService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<ExamineQueryService> logger)
    : QueryServiceBase(httpClient), IExamineQueryService
{
    private static readonly TimeSpan ContentCenterCacheDuration = TimeSpan.FromMinutes(2);

    protected override ILogger Logger => logger;

    private static readonly TimeSpan ListCacheDuration = TimeSpan.FromMinutes(2);

    private async Task<SdkResult<QueryResultModel<T>>> QueryListAsync<T>(
        string apiPath, string errorDomain, string displayName,
        QueryParameterModel parameter, CancellationToken cancellationToken) where T : class
    {
        var cacheKey = $"main-site:examine-list:{apiPath}:p{parameter.Page}:s{parameter.ItemsPerPage}:sb{string.Join(',', parameter.SortBy)}:sd{string.Join(',', parameter.SortDesc)}:q{parameter.SearchText}";
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<T>? cached) && cached is not null)
        {
            return SdkResult<QueryResultModel<T>>.Ok(cached);
        }

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
                memoryCache.Set(cacheKey, result, ListCacheDuration);
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

    // ─── 审核列表 ───

    public Task<SdkResult<QueryResultModel<ExamineOverviewModel>>> GetExaminesAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<ExamineOverviewModel>("api/examines/ListExamines", "EXAMINE", "审核", parameter, cancellationToken);

    // ─── 审阅记录列表 ───

    public Task<SdkResult<QueryResultModel<UserReviewEditRecordOverviewModel>>> GetUserReviewEditRecordsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserReviewEditRecordOverviewModel>("api/examines/ListUserReviewEditRecords", "EXAMINE_REVIEW", "审阅记录", parameter, cancellationToken);

    // ─── 用户监控列表 ───

    public Task<SdkResult<QueryResultModel<UserMonitorOverviewModel>>> GetUserMonitorsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
        => QueryListAsync<UserMonitorOverviewModel>("api/examines/ListUserMonitors", "EXAMINE_MONITOR", "监控", parameter, cancellationToken);

    // ─── 用户内容中心概览 ───

    public async Task<SdkResult<UserContentCenterViewModel>> GetUserContentCenterAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheKey = "main-site:user-content-center";
        if (memoryCache.TryGetValue(cacheKey, out UserContentCenterViewModel? cached) && cached is not null)
        {
            return SdkResult<UserContentCenterViewModel>.Ok(cached);
        }

        var result = await GetAsync<UserContentCenterViewModel>(
            "api/examines/GetUserContentCenterView",
            "EXAMINE_CONTENT_CENTER",
            "用户内容中心",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, ContentCenterCacheDuration);
        }

        return result;
    }

    // ─── 编辑记录概览 ───

    public Task<SdkResult<ExaminesOverviewViewModel>> GetExaminesOverviewAsync(
        long examineId, CancellationToken cancellationToken = default)
        => GetAsync<ExaminesOverviewViewModel>(
            $"api/examines/GetExaminesOverview/{examineId}",
            "EXAMINE_OVERVIEW",
            "编辑记录概览",
            cancellationToken);

    // ─── 单条审核详情 ───

    public Task<SdkResult<ExamineViewModel>> GetExamineDetailAsync(
        long examineId, CancellationToken cancellationToken = default)
        => GetAsync<ExamineViewModel>(
            $"api/examines/GetExamineView/{examineId}",
            "EXAMINE_DETAIL",
            "审核详情",
            cancellationToken);

    // ─── 对比审核（两个版本的 Before/After） ───

    public async Task<SdkResult<ExamineViewModel>> GetContrastExamineAsync(
        long contrastId, long currentId, CancellationToken cancellationToken = default)
    {
        // 分别加载两条审核记录的详情
        // BeforeModel 取自 contrastId 的 AfterModel（对比版本的终态）
        // AfterModel 取自 currentId 的 AfterModel（当前版本的终态）
        var contrastResult = await GetExamineDetailAsync(contrastId, cancellationToken);
        if (!contrastResult.Success || contrastResult.Data is null)
        {
            return contrastResult;
        }

        var currentResult = await GetExamineDetailAsync(currentId, cancellationToken);
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

    // ─── 词条编辑记录列表 ───

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetEntryEditRecordsAsync(
        int entryId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:entry-edit-records:{entryId}";
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
        var cacheKey = $"main-site:article-edit-records:{articleId}";
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
        var cacheKey = $"main-site:video-edit-records:{videoId}";
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
        var cacheKey = $"main-site:tag-edit-records:{tagId}";
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
        var cacheKey = $"main-site:periphery-edit-records:{peripheryId}";
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
