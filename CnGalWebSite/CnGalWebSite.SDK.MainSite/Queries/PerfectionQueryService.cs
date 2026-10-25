using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.Core.Services.Query;
using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class PerfectionQueryService(
    HttpClient httpClient,
    PublicDataCache publicCache,
    ILogger<PerfectionQueryService> logger) : QueryServiceBase(httpClient), IPerfectionQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<PerfectionLevelOverviewModel>> GetPerfectionLevelOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:typed-sort-v1:perfection-level-overview";
        if (memoryCache.TryGetValue(cacheKey, out PerfectionLevelOverviewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<PerfectionLevelOverviewModel>(cached, cacheRead);
        }

        var result = await GetAsync<PerfectionLevelOverviewModel>(
            "api/perfections/GetPerfectionLevelOverview",
            "PERFECTION_LEVEL_OVERVIEW",
            "完善度等级概览",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<PerfectionInforTipViewModel>>> GetPerfectionLevelRadomListAsync(
        PerfectionLevel level, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:typed-sort-v1:perfection-level-random:{level}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<PerfectionInforTipViewModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<PerfectionInforTipViewModel>>(cached, cacheRead);
        }

        var result = await GetAsync<IReadOnlyList<PerfectionInforTipViewModel>>(
            $"api/perfections/GetPerfectionLevelRadomList/{(int)level}",
            "PERFECTION_LEVEL_RANDOM",
            "完善度随机列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<IReadOnlyList<PerfectionCheckViewModel>>> GetPerfectionCheckLevelRadomListAsync(
        PerfectionCheckLevel level, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:typed-sort-v1:perfection-check-level-random:{level}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<PerfectionCheckViewModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<PerfectionCheckViewModel>>(cached, cacheRead);
        }

        var result = await GetAsync<IReadOnlyList<PerfectionCheckViewModel>>(
            $"api/perfections/GetPerfectionCheckLevelRadomList/{(int)level}",
            "PERFECTION_CHECK_LEVEL_RANDOM",
            "完善度检查随机列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<LineChartModel>> GetPerfectionLineChartAsync(
        LineChartType type, DateTime afterTime, DateTime beforeTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = $"api/perfections/GetPerfectionLineChart?type={type}&afterTime={new DateTimeOffset(afterTime).ToUnixTimeMilliseconds()}&beforeTime={new DateTimeOffset(beforeTime).ToUnixTimeMilliseconds()}";

        return await GetAsync<LineChartModel>(
            path,
            "PERFECTION_LINE_CHART",
            "完善度折线图",
            cancellationToken);
    }

    public async Task<SdkResult<IReadOnlyList<ExaminedNormalListModel>>> GetRecentlyEditListAsync(
        CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:typed-sort-v1:perfection-recently-edit";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<IReadOnlyList<ExaminedNormalListModel>>(cached, cacheRead);
        }

        var result = await GetAsync<IReadOnlyList<ExaminedNormalListModel>>(
            "api/perfections/GetRecentlyEditList",
            "PERFECTION_RECENTLY_EDIT",
            "最近编辑列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, CacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public async Task<SdkResult<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>> ListPerfectionsAsync(
        QueryParameterModel<PerfectionSortField> parameter, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:typed-sort-v1:perfections-list:p{parameter.Page}:s{parameter.ItemsPerPage}:sorts{SortCacheKey.Create(parameter.Sorts)}:q{parameter.SearchText}";
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<PerfectionOverviewModel, PerfectionSortField>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>(cached, cacheRead);
        }

        try
        {
            using var response = await HttpClient.PostAsJsonAsync(
                "api/perfections/ListPerfections", parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "完善度列表接口请求失败。Path=api/perfections/ListPerfections; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>.Fail(
                    "PERFECTION_LIST_HTTP_FAILED",
                    $"获取完善度列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>(body);
                cancellationToken.ThrowIfCancellationRequested();
                memoryCache.Set(cacheKey, result!, CacheDuration, cacheRead);
                cancellationToken.ThrowIfCancellationRequested();
                return memoryCache.Complete<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>(result!, cacheRead);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "完善度列表反序列化失败。Path=api/perfections/ListPerfections; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>.Fail(
                    "PERFECTION_LIST_DESERIALIZE_FAILED",
                    "完善度列表数据格式不兼容");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取完善度列表异常。Path=api/perfections/ListPerfections; BaseAddress={BaseAddress}",
                HttpClient.BaseAddress);
            return SdkFailure.FromException<QueryResultModel<PerfectionOverviewModel, PerfectionSortField>>(ex,
                "PERFECTION_LIST_EXCEPTION",
                "请求完善度列表时发生异常");
        }
    }

    public async Task<SdkResult<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>> ListPerfectionChecksAsync(
        QueryParameterModel<PerfectionCheckSortField> parameter, CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var cacheKey = $"main-site:typed-sort-v1:perfection-checks-list:p{parameter.Page}:s{parameter.ItemsPerPage}:sorts{SortCacheKey.Create(parameter.Sorts)}:q{parameter.SearchText}";
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>(cached, cacheRead);
        }

        try
        {
            using var response = await HttpClient.PostAsJsonAsync(
                "api/perfections/ListPerfectionChecks", parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "完善度检查列表接口请求失败。Path=api/perfections/ListPerfectionChecks; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>.Fail(
                    "PERFECTION_CHECK_LIST_HTTP_FAILED",
                    $"获取完善度检查列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>(body);
                cancellationToken.ThrowIfCancellationRequested();
                memoryCache.Set(cacheKey, result!, CacheDuration, cacheRead);
                cancellationToken.ThrowIfCancellationRequested();
                return memoryCache.Complete<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>(result!, cacheRead);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "完善度检查列表反序列化失败。Path=api/perfections/ListPerfectionChecks; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>.Fail(
                    "PERFECTION_CHECK_LIST_DESERIALIZE_FAILED",
                    "完善度检查列表数据格式不兼容");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取完善度检查列表异常。Path=api/perfections/ListPerfectionChecks; BaseAddress={BaseAddress}",
                HttpClient.BaseAddress);
            return SdkFailure.FromException<QueryResultModel<PerfectionCheckOverviewModel, PerfectionCheckSortField>>(ex,
                "PERFECTION_CHECK_LIST_EXCEPTION",
                "请求完善度检查列表时发生异常");
        }
    }
}
