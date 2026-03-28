using System.Net.Http.Json;
using System.Text.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tables;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

/// <summary>
/// 数据汇总查询服务实现。
/// </summary>
public class TableQueryService(HttpClient httpClient, IMemoryCache memoryCache, ILogger<TableQueryService> logger)
    : QueryServiceBase(httpClient), ITableQueryService
{
    private static readonly TimeSpan SummaryCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan QueryCacheDuration = TimeSpan.FromMinutes(3);

    protected override ILogger Logger { get; } = logger;

    public async Task<SdkResult<TableViewModel>> GetTableSummaryAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "main-site:table-summary";
        if (memoryCache.TryGetValue(cacheKey, out TableViewModel? cached) && cached is not null)
        {
            return SdkResult<TableViewModel>.Ok(cached);
        }

        var result = await GetAsync<TableViewModel>(
            "api/tables/GetTableView",
            "TABLE",
            "数据汇总概要",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, SummaryCacheDuration);
        }

        return result;
    }

    public Task<SdkResult<QueryResultModel<BasicInforTableModel>>> QueryGamesAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<BasicInforTableModel>("api/tables/ListGames", request, "游戏列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<GroupInforTableModel>>> QueryGroupsAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<GroupInforTableModel>("api/tables/ListGroups", request, "制作组列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<StaffInforTableModel>>> QueryStaffsAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<StaffInforTableModel>("api/tables/ListStaffs", request, "STAFF列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<MakerInforTableModel>>> QueryMakersAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<MakerInforTableModel>("api/tables/ListMakers", request, "制作人列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<RoleInforTableModel>>> QueryRolesAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<RoleInforTableModel>("api/tables/ListRoles", request, "角色列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<StoreInfoViewModel>>> QueryStoreInfoAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<StoreInfoViewModel>("api/tables/ListStoreInfo", request, "游戏价格列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<GameScoreTableModel>>> QueryGameScoresAsync(
        QueryParameterModel request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<GameScoreTableModel>("api/tables/ListGameScores", request, "游戏评分列表", cancellationToken);
    }

    // ── 缓存键生成 ──

    private static string BuildCacheKey(string path, QueryParameterModel request)
    {
        var sortBy = request.SortBy != null ? string.Join(",", request.SortBy) : "";
        var sortDesc = request.SortDesc != null ? string.Join(",", request.SortDesc) : "";
        return $"main-site:table:{path}:p{request.Page}:s{request.ItemsPerPage}:sb{sortBy}:sd{sortDesc}:q{request.SearchText}";
    }

    // ── 带缓存的 POST 分页查询 ──

    private async Task<SdkResult<QueryResultModel<T>>> PostQueryWithCacheAsync<T>(
        string path,
        QueryParameterModel request,
        string entityDescription,
        CancellationToken cancellationToken) where T : class
    {
        var cacheKey = BuildCacheKey(path, request);
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<T>? cached) && cached is not null)
        {
            return SdkResult<QueryResultModel<T>>.Ok(cached);
        }

        var result = await PostQueryAsync<T>(path, request, entityDescription, cancellationToken);

        if (result.Success && result.Data is not null)
        {
            memoryCache.Set(cacheKey, result.Data, QueryCacheDuration);
        }

        return result;
    }

    // ── 通用 POST 分页查询 ──

    private async Task<SdkResult<QueryResultModel<T>>> PostQueryAsync<T>(
        string path,
        QueryParameterModel request,
        string entityDescription,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync(path, request, SdkJsonSerializerOptions.Default, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "获取{EntityDescription}失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<QueryResultModel<T>>.Fail("TABLE_QUERY_FAILED", $"获取{entityDescription}失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var data = Deserialize<QueryResultModel<T>>(responseBody);
                if (data is null)
                {
                    return SdkResult<QueryResultModel<T>>.Fail("TABLE_EMPTY_RESPONSE", $"{entityDescription}数据为空");
                }
                return SdkResult<QueryResultModel<T>>.Ok(data);
            }
            catch (JsonException ex)
            {
                Logger.LogError(
                    ex,
                    "{EntityDescription}反序列化失败。Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    path,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<QueryResultModel<T>>.Fail("TABLE_DESERIALIZE_FAILED", $"{entityDescription}数据格式不兼容");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "获取{EntityDescription}异常。Path={Path}; BaseAddress={BaseAddress}",
                entityDescription,
                path,
                HttpClient.BaseAddress);
            return SdkResult<QueryResultModel<T>>.Fail("TABLE_QUERY_EXCEPTION", $"请求{entityDescription}时发生异常");
        }
    }
}

