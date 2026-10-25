using System.Net.Http.Json;
using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

internal static class SdkResponse
{
    public static async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!response.IsSuccessStatusCode)
        {
            var status = (int)response.StatusCode;
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            // Only an explicit client-side business rejection is safe to display.
            // Proxy pages, arbitrary JSON and server failures keep the generic HTTP error.
            if (status is >= 400 and < 500
                && (string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase)
                    || mediaType?.EndsWith("+json", StringComparison.OrdinalIgnoreCase) == true))
            {
                BusinessFailure? failure = null;
                try
                {
                    failure = await response.Content.ReadFromJsonAsync<BusinessFailure>(
                        SdkJsonSerializerOptions.Default, cancellationToken);
                }
                catch (JsonException) { }

                if (failure is { Successful: false } && !string.IsNullOrWhiteSpace(failure.Error))
                    throw new SdkBusinessException(failure.Error, status);
            }

            response.EnsureSuccessStatusCode();
        }

        return await response.Content.ReadFromJsonAsync<T>(SdkJsonSerializerOptions.Default, cancellationToken);
    }

    private sealed record BusinessFailure(bool? Successful, string? Error);
}

internal sealed class SdkBusinessException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
