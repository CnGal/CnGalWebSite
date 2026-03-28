using System.Text.Json;
using System.Text.Json.Serialization;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public static class SdkJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 带缩进的序列化配置，用于数据导出等需要可读格式的场景。
    /// 与 Default 共享相同的基础配置（camelCase、枚举转换器）。
    /// </summary>
    public static readonly JsonSerializerOptions Indented = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    static SdkJsonSerializerOptions()
    {
        // 兼容 APIServer 返回的字符串枚举值（如 "None"）
        Default.Converters.Add(new JsonStringEnumConverter());
        Indented.Converters.Add(new JsonStringEnumConverter());
    }
}
