using System.Text.Json;
using System.Text.Json.Serialization;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

internal static class SdkJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    static SdkJsonSerializerOptions()
    {
        // 兼容 APIServer 返回的字符串枚举值（如 "None"）
        Default.Converters.Add(new JsonStringEnumConverter());
    }
}
