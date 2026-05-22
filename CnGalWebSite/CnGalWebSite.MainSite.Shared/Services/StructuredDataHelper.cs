using System.Text.Json;
using System.Text.Json.Nodes;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 提供生成 JSON-LD 结构化数据时的通用方法。
/// </summary>
public static class StructuredDataHelper
{
    /// <summary>
    /// 将 <see cref="JsonObject"/> 序列化为单行 JSON 字符串，自动处理所有转义。
    /// </summary>
    public static string SerializeJsonLd(JsonObject data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    public static string LogoUrl => "https://app.cngal.org/_content/CnGalWebSite.Shared/images/logo.png";
}
