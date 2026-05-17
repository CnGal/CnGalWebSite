using System.Text;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 提供生成 JSON-LD 结构化数据时的常用转义方法。
/// </summary>
public static class StructuredDataHelper
{
    /// <summary>
    /// 对 JSON 字符串值中的特殊字符进行转义，返回安全的值（不含外层双引号）。
    /// </summary>
    public static string EscapeJson(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }

    public static string LogoUrl => "https://app.cngal.org/_content/CnGalWebSite.Shared/images/logo.png";
}
