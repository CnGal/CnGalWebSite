using System.Net.Http.Json;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

/// <summary>
/// Command 服务基类，封装通用的 POST/GET + JSON 序列化辅助方法。
/// </summary>
public abstract class CommandServiceBase(HttpClient httpClient)
{
    protected HttpClient HttpClient { get; } = httpClient;

    /// <summary>
    /// 子类必须暴露自身的 Logger，供辅助方法使用。
    /// </summary>
    protected abstract ILogger Logger { get; }

    /// <summary>
    /// GET 请求并使用统一序列化配置反序列化。
    /// </summary>
    protected Task<T?> GetFromJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
    {
        return HttpClient.GetFromJsonAsync<T>(requestUri, SdkJsonSerializerOptions.Default, cancellationToken);
    }

    /// <summary>
    /// POST JSON 请求并使用统一序列化配置反序列化响应。
    /// </summary>
    protected async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var response = await HttpClient.PostAsJsonAsync(requestUri, request, SdkJsonSerializerOptions.Default, cancellationToken);
        return await response.Content.ReadFromJsonAsync<TResponse>(SdkJsonSerializerOptions.Default, cancellationToken);
    }

    /// <summary>
    /// POST JSON 请求并返回原始 HttpResponseMessage（用于需要检查状态码的场景）。
    /// </summary>
    protected Task<HttpResponseMessage> PostAsJsonRawAsync<TRequest>(
        string requestUri,
        TRequest request,
        CancellationToken cancellationToken)
    {
        return HttpClient.PostAsJsonAsync(requestUri, request, SdkJsonSerializerOptions.Default, cancellationToken);
    }

    /// <summary>
    /// 从 HttpResponseMessage 反序列化响应体。
    /// </summary>
    protected static Task<T?> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        return response.Content.ReadFromJsonAsync<T>(SdkJsonSerializerOptions.Default, cancellationToken);
    }
}
