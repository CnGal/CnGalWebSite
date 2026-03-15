using System.Text.Json;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public abstract class QueryServiceBase(HttpClient httpClient)
{
    protected HttpClient HttpClient { get; } = httpClient;

    /// <summary>
    /// 子类必须暴露自身的 Logger，供模板方法使用。
    /// </summary>
    protected abstract ILogger Logger { get; }

    // ──────── 底层辅助 ────────

    protected static async Task<(HttpResponseMessage Response, string Body)> GetAsyncWithBody(
        HttpClient httpClient,
        string path,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(path, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return (response, body);
    }

    protected static T? Deserialize<T>(string responseBody)
    {
        return JsonSerializer.Deserialize<T>(responseBody, SdkJsonSerializerOptions.Default);
    }

    protected static string TrimForLog(string text, int maxLength = 800)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return text.Length <= maxLength ? text : $"{text[..maxLength]}...(truncated)";
    }

    // ──────── 模板方法：获取单个实体 ────────

    /// <summary>
    /// 通用 GET → 反序列化 → 映射 → SdkResult 流程。
    /// 自动处理 404、HTTP 失败、反序列化异常、null 响应、运行时异常。
    /// </summary>
    /// <typeparam name="TDto">API 返回的原始 DTO 类型</typeparam>
    /// <typeparam name="TResult">对外暴露的 ViewModel 类型</typeparam>
    /// <param name="path">API 路径</param>
    /// <param name="mapFunc">DTO → ViewModel 映射函数</param>
    /// <param name="errorCodePrefix">错误码前缀，如 "ENTRY"、"SPACE"</param>
    /// <param name="entityDescription">日志中使用的实体描述，如 "条目"、"用户空间"</param>
    /// <param name="entityId">实体标识（用于日志，可为 int/string）</param>
    /// <param name="cancellationToken"></param>
    protected async Task<SdkResult<TResult>> GetSingleAsync<TDto, TResult>(
        string path,
        Func<TDto, TResult> mapFunc,
        string errorCodePrefix,
        string entityDescription,
        object? entityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Logger.LogWarning(
                    "{EntityDescription}不存在。EntityId={EntityId}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    entityId,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<TResult>.Fail($"{errorCodePrefix}_NOT_FOUND", $"未找到对应{entityDescription}", (int)response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "{EntityDescription}接口请求失败。EntityId={EntityId}; Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    entityId,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<TResult>.Fail($"{errorCodePrefix}_QUERY_FAILED", $"获取{entityDescription}详情失败", (int)response.StatusCode);
            }

            TDto? dto;
            try
            {
                dto = Deserialize<TDto>(responseBody);
            }
            catch (JsonException ex)
            {
                Logger.LogError(
                    ex,
                    "{EntityDescription}接口反序列化失败。EntityId={EntityId}; Path={Path}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    entityId,
                    path,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));
                return SdkResult<TResult>.Fail($"{errorCodePrefix}_INVALID_RESPONSE", $"{entityDescription}数据格式不兼容", 200);
            }

            if (dto is null)
            {
                return SdkResult<TResult>.Fail($"{errorCodePrefix}_EMPTY_RESPONSE", $"{entityDescription}数据为空");
            }

            return SdkResult<TResult>.Ok(mapFunc(dto));
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "获取{EntityDescription}详情异常。EntityId={EntityId}; Path={Path}; BaseAddress={BaseAddress}",
                entityDescription,
                entityId,
                path,
                HttpClient.BaseAddress);
            return SdkResult<TResult>.Fail($"{errorCodePrefix}_QUERY_EXCEPTION", $"请求{entityDescription}详情时发生异常");
        }
    }

    // ──────── 模板方法：获取列表（降级为空集合） ────────

    /// <summary>
    /// 通用 GET 列表请求，失败时降级为空集合并将错误追加到 warnings。
    /// 用于 Home 等聚合页面的"部分可用"策略。
    /// </summary>
    protected async Task<IReadOnlyList<TItem>> GetListSafeAsync<TItem>(
        string path,
        string errorCode,
        ICollection<SdkErrorModel> warnings,
        CancellationToken cancellationToken)
    {
        try
        {
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "聚合接口请求失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}; ResponseBody={ResponseBody}",
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    errorCode,
                    TrimForLog(responseBody));

                warnings.Add(new SdkErrorModel
                {
                    Code = errorCode,
                    Message = $"请求 {path} 失败（HTTP {(int)response.StatusCode}）",
                    StatusCode = (int)response.StatusCode
                });
                return [];
            }

            try
            {
                var data = Deserialize<List<TItem>>(responseBody);
                return data ?? [];
            }
            catch (JsonException ex)
            {
                Logger.LogError(
                    ex,
                    "聚合接口反序列化失败。Path={Path}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}; ResponseBody={ResponseBody}",
                    path,
                    HttpClient.BaseAddress,
                    errorCode,
                    TrimForLog(responseBody));
                warnings.Add(new SdkErrorModel
                {
                    Code = errorCode,
                    Message = $"请求 {path} 成功但数据格式不兼容",
                    StatusCode = 200
                });
                return [];
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "聚合接口请求异常。Path={Path}; BaseAddress={BaseAddress}; ErrorCode={ErrorCode}",
                path,
                HttpClient.BaseAddress,
                errorCode);
            warnings.Add(new SdkErrorModel
            {
                Code = errorCode,
                Message = $"请求 {path} 时发生异常",
                StatusCode = null
            });
            return [];
        }
    }

    // ──────── 模板方法：简单 GET → SdkResult<T> ────────

    /// <summary>
    /// 通用 GET → 反序列化 → SdkResult 流程（不需要 DTO→ViewModel 映射的场景）。
    /// </summary>
    protected async Task<SdkResult<T>> GetAsync<T>(
        string path,
        string errorCodePrefix,
        string entityDescription,
        CancellationToken cancellationToken)
    {
        try
        {
            var (response, responseBody) = await GetAsyncWithBody(HttpClient, path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError(
                    "获取{EntityDescription}失败。Path={Path}; StatusCode={StatusCode}; BaseAddress={BaseAddress}; ResponseBody={ResponseBody}",
                    entityDescription,
                    path,
                    (int)response.StatusCode,
                    HttpClient.BaseAddress,
                    TrimForLog(responseBody));

                return SdkResult<T>.Fail($"{errorCodePrefix}_HTTP_FAILED", $"获取{entityDescription}失败（HTTP {(int)response.StatusCode}）");
            }

            try
            {
                var data = Deserialize<T>(responseBody);
                return SdkResult<T>.Ok(data!);
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

                return SdkResult<T>.Fail($"{errorCodePrefix}_DESERIALIZE_FAILED", $"{entityDescription}数据格式不兼容");
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

            return SdkResult<T>.Fail($"{errorCodePrefix}_EXCEPTION", $"请求{entityDescription}数据时发生异常");
        }
    }
}
