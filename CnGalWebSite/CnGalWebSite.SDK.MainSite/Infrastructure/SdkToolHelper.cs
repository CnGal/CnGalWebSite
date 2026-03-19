using System;
using System.Text;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public static class SdkToolHelper
{
    public static string Base64EncodeUrl(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return "A" + Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
    }

    public static string Base64DecodeUrl(string content)
    {
        content = content[1..];
        content = content.Replace("-", "+").Replace("_", "/").Replace("%3d", "=").Replace("%3D", "=");
        var bytes = Convert.FromBase64String(content);
        var result = Encoding.UTF8.GetString(bytes);
        
        // Validate Url
        if (string.IsNullOrWhiteSpace(result))
        {
            return result;
        }
        else
        {
            if (result.StartsWith("/"))
            {
                return result;
            }
            else
            {
                if (result.StartsWith("https://app.cngal.org") || result.StartsWith("http://app.cngal.org") || result.StartsWith("https://m.cngal.org") || result.StartsWith("http://m.cngal.org") || result.StartsWith("https://www.cngal.org") || result.StartsWith("http://www.cngal.org") || result.StartsWith("https://localhost:") || result.StartsWith("http://localhost:"))
                {
                    return result;
                }
                else
                {
                    return "/";
                }
            }
        }
    }
}
