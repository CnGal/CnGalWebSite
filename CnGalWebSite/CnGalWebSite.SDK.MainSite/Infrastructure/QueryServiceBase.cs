using System.Text.Json;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public abstract class QueryServiceBase(HttpClient httpClient)
{
    protected HttpClient HttpClient { get; } = httpClient;

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
}
