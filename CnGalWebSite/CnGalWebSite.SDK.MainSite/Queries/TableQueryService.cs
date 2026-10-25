using CnGalWebSite.DataModel.Sorting;
using CnGalWebSite.Core.Services.Query;
using System.Net.Http.Json;
using System.Text.Json;
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Steam;
using CnGalWebSite.DataModel.ViewModel.Tables;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

/// <summary>
/// 数据汇总查询服务实现。
/// </summary>
public class TableQueryService(HttpClient httpClient, PublicDataCache publicCache, ILogger<TableQueryService> logger)
    : QueryServiceBase(httpClient), ITableQueryService
{
    private readonly PublicDataCache memoryCache = publicCache.ForSource(httpClient.BaseAddress);

    private static readonly TimeSpan SummaryCacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan QueryCacheDuration = TimeSpan.FromMinutes(3);

    protected override ILogger Logger { get; } = logger;

    public async Task<SdkResult<TableViewModel>> GetTableSummaryAsync(CancellationToken cancellationToken = default)
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        const string cacheKey = "main-site:typed-sort-v1:table-summary";
        if (memoryCache.TryGetValue(cacheKey, out TableViewModel? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<TableViewModel>(cached, cacheRead);
        }

        var result = await GetAsync<TableViewModel>(
            "api/tables/GetTableView",
            "TABLE",
            "数据汇总概要",
            cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, SummaryCacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    public Task<SdkResult<QueryResultModel<BasicInforTableModel, BasicInforSortField>>> QueryGamesAsync(
        QueryParameterModel<BasicInforSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<BasicInforTableModel, BasicInforSortField>("api/tables/ListGames", request, "游戏列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<GroupInforTableModel, GroupInforSortField>>> QueryGroupsAsync(
        QueryParameterModel<GroupInforSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<GroupInforTableModel, GroupInforSortField>("api/tables/ListGroups", request, "制作组列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<StaffInforTableModel, StaffInforSortField>>> QueryStaffsAsync(
        QueryParameterModel<StaffInforSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<StaffInforTableModel, StaffInforSortField>("api/tables/ListStaffs", request, "STAFF列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<MakerInforTableModel, MakerInforSortField>>> QueryMakersAsync(
        QueryParameterModel<MakerInforSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<MakerInforTableModel, MakerInforSortField>("api/tables/ListMakers", request, "制作人列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<RoleInforTableModel, RoleInforSortField>>> QueryRolesAsync(
        QueryParameterModel<RoleInforSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<RoleInforTableModel, RoleInforSortField>("api/tables/ListRoles", request, "角色列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<StoreInfoViewModel, StoreTableSortField>>> QueryStoreInfoAsync(
        QueryParameterModel<StoreTableSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<StoreInfoViewModel, StoreTableSortField>("api/tables/ListStoreInfo", request, "游戏价格列表", cancellationToken);
    }

    public Task<SdkResult<QueryResultModel<GameScoreTableModel, GameScoreSortField>>> QueryGameScoresAsync(
        QueryParameterModel<GameScoreSortField> request, CancellationToken cancellationToken = default)
    {
        return PostQueryWithCacheAsync<GameScoreTableModel, GameScoreSortField>("api/tables/ListGameScores", request, "游戏评分列表", cancellationToken);
    }

    // ── 缓存键生成 ──

    private static string BuildCacheKey<TSort>(string path, QueryParameterModel<TSort> request) where TSort : struct, Enum
    {
        return $"main-site:typed-sort-v1:table:{path}:p{request.Page}:s{request.ItemsPerPage}:sorts{SortCacheKey.Create(request.Sorts)}:q{request.SearchText}";
    }

    // ── 带缓存的 POST 分页查询 ──

    private async Task<SdkResult<QueryResultModel<T, TSort>>> PostQueryWithCacheAsync<T, TSort>(
        string path,
        QueryParameterModel<TSort> request,
        string entityDescription,
        CancellationToken cancellationToken) where T : class where TSort : struct, Enum
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        var cacheKey = BuildCacheKey(path, request);
        if (memoryCache.TryGetValue(cacheKey, out QueryResultModel<T, TSort>? cached, cacheRead) && cached is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return memoryCache.Complete<QueryResultModel<T, TSort>>(cached, cacheRead);
        }

        var result = await PostQueryAsync<T, TSort>(path, request, entityDescription, cancellationToken);

        if (result.Success && result.Data is not null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            memoryCache.Set(cacheKey, result.Data, QueryCacheDuration, cacheRead);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return memoryCache.CompleteResult(result, cacheRead);
    }

    // ── 通用 POST 分页查询 ──

    private async Task<SdkResult<QueryResultModel<T, TSort>>> PostQueryAsync<T, TSort>(
        string path,
        QueryParameterModel<TSort> request,
        string entityDescription,
        CancellationToken cancellationToken) where T : class where TSort : struct, Enum
    {
        var cacheRead = await memoryCache.BeginQueryAsync(cancellationToken);
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(path, request, SdkJsonSerializerOptions.Default, cancellationToken);
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
                return SdkResult<QueryResultModel<T, TSort>>.Fail("TABLE_QUERY_FAILED", $"获取{entityDescription}失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var data = Deserialize<QueryResultModel<T, TSort>>(responseBody);
                if (data is null)
                {
                    return SdkResult<QueryResultModel<T, TSort>>.Fail("TABLE_EMPTY_RESPONSE", $"{entityDescription}数据为空");
                }
                cancellationToken.ThrowIfCancellationRequested();
                return memoryCache.Complete<QueryResultModel<T, TSort>>(data, cacheRead);
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
                return SdkResult<QueryResultModel<T, TSort>>.Fail("TABLE_DESERIALIZE_FAILED", $"{entityDescription}数据格式不兼容");
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "获取{EntityDescription}异常。Path={Path}; BaseAddress={BaseAddress}",
                entityDescription,
                path,
                HttpClient.BaseAddress);
            return SdkFailure.FromException<QueryResultModel<T, TSort>>(ex, "TABLE_QUERY_EXCEPTION", $"请求{entityDescription}时发生异常");
        }
    }
}

