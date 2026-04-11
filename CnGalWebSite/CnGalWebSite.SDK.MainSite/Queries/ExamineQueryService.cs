using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.EditRecords;
using CnGalWebSite.DataModel.ViewModel.Home;
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
}
