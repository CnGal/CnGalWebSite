using Microsoft.Extensions.Options;

namespace CnGalWebSite.Core.Configuration;

public static class ConfigurationOptions
{
    // 可选功能在运行期间读取配置。将框架绑定或校验失败转换为经过脱敏的应用层配置异常。
    public static T GetOptional<T>(this IOptions<T> options, string section)
        where T : class
    {
        try
        {
            return options.Value;
        }
        catch (OptionsValidationException)
        {
            throw new ConfigurationException(section);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or
            FormatException or OverflowException or InvalidCastException)
        {
            throw new ConfigurationException(section);
        }
    }
}
