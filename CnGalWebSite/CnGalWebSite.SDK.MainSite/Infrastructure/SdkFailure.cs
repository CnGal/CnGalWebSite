using System.Text.Json;
using CnGalWebSite.SDK.MainSite.Models;
using Polly.Timeout;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

internal static class SdkFailure
{
    public static SdkResult<T> FromException<T>(Exception exception, string code, string message, int? statusCode = null)
    {
        if (exception is SdkIdentityChangedException)
            return SdkResult<T>.Fail("SDK_IDENTITY_CHANGED", exception.Message);
        if (exception is TimeoutException or TimeoutRejectedException or OperationCanceledException)
            return SdkResult<T>.Fail(code.Replace("_EXCEPTION", "_TIMEOUT"), "请求超时，请稍后重试");
        if (exception is HttpRequestException http)
            return SdkResult<T>.Fail(code.Replace("_EXCEPTION", "_HTTP_FAILED"), "接口请求失败", (int?)http.StatusCode);
        if (exception is JsonException)
            return SdkResult<T>.Fail(code.Replace("_EXCEPTION", "_INVALID_RESPONSE"), "接口返回的数据格式不兼容", statusCode);
        return SdkResult<T>.Fail(code, message, statusCode);
    }
}

public sealed class SdkIdentityChangedException() : Exception("请求期间身份已改变，请重新加载页面");
