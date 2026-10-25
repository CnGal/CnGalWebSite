using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>Request-local rendering outcome; the host alone decides HTTP and CDN policy.</summary>
public sealed class PageResponseState
{
    public bool Ready { get; private set; }
    public int? FailureStatusCode { get; private set; }
    public void Begin() { Ready = false; FailureStatusCode = null; }
    public void Succeeded() { Ready = true; FailureStatusCode = null; }
    public void Failed(SdkErrorModel? error) => Failed(error?.StatusCode is >= 400 and <= 599 ? error.StatusCode.Value : 503);
    public void Failed(int statusCode) { Ready = false; FailureStatusCode = statusCode; }
}
