using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 提供生成 JSON-LD 结构化数据时的通用方法。
/// </summary>
public static class StructuredDataHelper
{
    /// <summary>
    /// 将 <see cref="JsonObject"/> 序列化为单行 JSON 字符串。
    /// 输出经过 JavaScriptEncoder.Default 转义（&lt; &gt; &amp; 等 HTML 敏感字符转义为 \uXXXX），
    /// 配合 CgStructuredData 中 MarkupString 原样输出时可安全嵌入 &lt;script&gt; 标签。
    /// </summary>
    public static string SerializeJsonLd(JsonObject data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Default
        });
    }

    public static string LogoUrl => "https://app.cngal.org/_content/CnGalWebSite.Shared/images/logo.png";
}
