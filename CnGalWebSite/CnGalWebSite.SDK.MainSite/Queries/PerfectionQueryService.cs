using System.Net.Http.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

public sealed class PerfectionQueryService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<PerfectionQueryService> logger) : QueryServiceBase(httpClient), IPerfectionQueryService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    protected override ILogger Logger => logger;

    public async Task<SdkResult<PerfectionLevelOverviewModel>> GetPerfectionLevelOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:perfection-level-overview";
        if (memoryCache.TryGetValue(cacheKey, out PerfectionLevelOverviewModel? cached) && cached is not null)
        {
            return SdkResult<PerfectionLevelOverviewModel>.Ok(cached);
        }

        var result = await GetAsync<PerfectionLevelOverviewModel>(
            "api/perfections/GetPerfectionLevelOverview",
            "PERFECTION_LEVEL_OVERVIEW",
            "完善度等级概览",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, CacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<IReadOnlyList<PerfectionInforTipViewModel>>> GetPerfectionLevelRadomListAsync(
        PerfectionLevel level, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:perfection-level-random:{level}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<PerfectionInforTipViewModel>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<PerfectionInforTipViewModel>>.Ok(cached);
        }

        var result = await GetAsync<IReadOnlyList<PerfectionInforTipViewModel>>(
            $"api/perfections/GetPerfectionLevelRadomList/{(int)level}",
            "PERFECTION_LEVEL_RANDOM",
            "完善度随机列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, CacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<IReadOnlyList<PerfectionCheckViewModel>>> GetPerfectionCheckLevelRadomListAsync(
        PerfectionCheckLevel level, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:perfection-check-level-random:{level}";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<PerfectionCheckViewModel>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<PerfectionCheckViewModel>>.Ok(cached);
        }

        var result = await GetAsync<IReadOnlyList<PerfectionCheckViewModel>>(
            $"api/perfections/GetPerfectionCheckLevelRadomList/{(int)level}",
            "PERFECTION_CHECK_LEVEL_RANDOM",
            "完善度检查随机列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, CacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<LineChartModel>> GetPerfectionLineChartAsync(
        LineChartType type, DateTime afterTime, DateTime beforeTime, CancellationToken cancellationToken = default)
    {
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
        const string cacheKey = "main-site:perfection-recently-edit";
        if (memoryCache.TryGetValue(cacheKey, out IReadOnlyList<ExaminedNormalListModel>? cached) && cached is not null)
        {
            return SdkResult<IReadOnlyList<ExaminedNormalListModel>>.Ok(cached);
        }

        var result = await GetAsync<IReadOnlyList<ExaminedNormalListModel>>(
            "api/perfections/GetRecentlyEditList",
            "PERFECTION_RECENTLY_EDIT",
            "最近编辑列表",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, CacheDuration);
        }

        return result;
    }

    public async Task<SdkResult<QueryResultModel<PerfectionOverviewModel>>> ListPerfectionsAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:perfections-list:p{parameter.Page}:s{parameter.ItemsPerPage}:sb{string.Join(',', parameter.SortBy)}:sd{string.Join(',', parameter.SortDesc)}:q{parameter.SearchText}";
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<PerfectionOverviewModel>? cached) && cached is not null)
        {
            return SdkResult<QueryResultModel<PerfectionOverviewModel>>.Ok(cached);
        }

        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                "api/perfections/ListPerfections", parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "完善度列表接口请求失败。Path=api/perfections/ListPerfections; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionOverviewModel>>.Fail(
                    "PERFECTION_LIST_HTTP_FAILED",
                    $"获取完善度列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<PerfectionOverviewModel>>(body);
                memoryCache.Set(cacheKey, result!, CacheDuration);
                return SdkResult<QueryResultModel<PerfectionOverviewModel>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "完善度列表反序列化失败。Path=api/perfections/ListPerfections; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionOverviewModel>>.Fail(
                    "PERFECTION_LIST_DESERIALIZE_FAILED",
                    "完善度列表数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取完善度列表异常。Path=api/perfections/ListPerfections; BaseAddress={BaseAddress}",
                HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<PerfectionOverviewModel>>.Fail(
                "PERFECTION_LIST_EXCEPTION",
                "请求完善度列表时发生异常");
        }
    }

    public async Task<SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>> ListPerfectionChecksAsync(
        QueryParameterModel parameter, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"main-site:perfection-checks-list:p{parameter.Page}:s{parameter.ItemsPerPage}:sb{string.Join(',', parameter.SortBy)}:sd{string.Join(',', parameter.SortDesc)}:q{parameter.SearchText}";
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<PerfectionCheckOverviewModel>? cached) && cached is not null)
        {
            return SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>.Ok(cached);
        }

        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                "api/perfections/ListPerfectionChecks", parameter, SdkJsonSerializerOptions.Default, cancellationToken);

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "完善度检查列表接口请求失败。Path=api/perfections/ListPerfectionChecks; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    (int)response.StatusCode, HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>.Fail(
                    "PERFECTION_CHECK_LIST_HTTP_FAILED",
                    $"获取完善度检查列表失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var result = Deserialize<QueryResultModel<PerfectionCheckOverviewModel>>(body);
                memoryCache.Set(cacheKey, result!, CacheDuration);
                return SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>.Ok(result!);
            }
            catch (System.Text.Json.JsonException ex)
            {
                Logger.LogError(ex,
                    "完善度检查列表反序列化失败。Path=api/perfections/ListPerfectionChecks; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    HttpClient.BaseAddress, TrimForLog(body));
                return SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>.Fail(
                    "PERFECTION_CHECK_LIST_DESERIALIZE_FAILED",
                    "完善度检查列表数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "获取完善度检查列表异常。Path=api/perfections/ListPerfectionChecks; BaseAddress={BaseAddress}",
                HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<PerfectionCheckOverviewModel>>.Fail(
                "PERFECTION_CHECK_LIST_EXCEPTION",
                "请求完善度检查列表时发生异常");
        }
    }
}
