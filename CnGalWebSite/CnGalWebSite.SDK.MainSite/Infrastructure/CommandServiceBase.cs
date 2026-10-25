using System.Net.Http.Json;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

/// <summary>
/// Command 服务基类，封装通用的 POST/GET + JSON 序列化辅助方法。
/// </summary>
public abstract class CommandServiceBase(HttpClient httpClient, PublicDataCache publicCache)
{
    protected HttpClient HttpClient { get; } = httpClient;
    protected PublicDataCache PublicCache { get; } = publicCache.ForSource(httpClient.BaseAddress);

    /// <summary>
    /// 子类必须暴露自身的 Logger，供辅助方法使用。
    /// </summary>
    protected abstract ILogger Logger { get; }

    /// <summary>
    /// GET 请求并使用统一序列化配置反序列化。
    /// </summary>
    protected async Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        using var request = SdkRequestPolicy.Read(HttpMethod.Get, requestUri);
        using var response = await HttpClient.SendAsync(request, cancellationToken);
        return await ReadResponseAsync<T>(response, cancellationToken);
    }

    /// <summary>
    /// POST JSON 请求并使用统一序列化配置反序列化响应。
    /// </summary>
    protected async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await PostAsJsonRawAsync(requestUri, request, cancellationToken);
        return await ReadResponseAsync<TResponse>(response, cancellationToken);
    }

    /// <summary>
    /// POST JSON 请求并返回原始 HttpResponseMessage（用于需要检查状态码的场景）。
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsJsonRawAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken,
        bool readOnly = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await HttpClient.PostAsJsonAsync(requestUri, request, SdkJsonSerializerOptions.Default, cancellationToken);
        }
        finally
        {
            // A lost response can still mean the write committed. Invalidate cached data and prevent stale refill.
            if (!readOnly) PublicCache.Invalidate();
        }
    }

    /// <summary>
    /// 从 HttpResponseMessage 反序列化响应体。
    /// </summary>
    protected static Task<T?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return SdkResponse.ReadAsync<T>(response, cancellationToken);
    }
}
