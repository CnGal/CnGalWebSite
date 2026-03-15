namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class SdkResult<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public SdkErrorModel? Error { get; init; }

    public static SdkResult<T> Ok(T data)
    {
        return new SdkResult<T>
        {
            Success = true,
            Data = data
        };
    }

    public static SdkResult<T> Fail(string code, string message, int? statusCode = null)
    {
        return new SdkResult<T>
        {
            Success = false,
            Error = new SdkErrorModel
            {
                Code = code,
                Message = message,
                StatusCode = statusCode
            }
        };
    }
}

public sealed class SdkErrorModel
{
    public required string Code { get; init; }

    public required string Message { get; init; }

    public int? StatusCode { get; init; }
}
