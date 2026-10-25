using System;
using System.Linq;

namespace CnGalWebSite.Extensions;

/// <summary>
/// URL output safety rules shared by server and UI projects.
/// This validates values before they are placed into href/src attributes; it is
/// intentionally stricter than <see cref="Uri.TryCreate(string?, UriKind, out Uri?)"/>,
/// which accepts javascript:, data:, vbscript: and other executable schemes.
/// </summary>
public static class UrlSafety
{
    /// <summary>
    /// Returns a value that is safe to emit as an href/src attribute, or null.
    /// Allowed values are absolute http(s) URLs and root-relative site paths.
    /// </summary>
    public static string? GetSafeHref(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var value = url.Trim();
        if (value.Any(char.IsControl))
        {
            return null;
        }

        if (IsRootRelativePath(value))
        {
            return value;
        }

        return TryCreateHttpUri(value, out _) ? value : null;
    }

    /// <summary>
    /// True only for absolute http(s) URLs. Root-relative paths are not external.
    /// </summary>
    public static bool IsExternalHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return TryCreateHttpUri(url.Trim(), out _);
    }

    private static bool IsRootRelativePath(string value) =>
        value.StartsWith("/", StringComparison.Ordinal)
        && !value.StartsWith("//", StringComparison.Ordinal)
        && !value.StartsWith("/\\", StringComparison.Ordinal);

    private static bool TryCreateHttpUri(string value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed)
            && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps)
            && string.IsNullOrWhiteSpace(parsed.Host) is false)
        {
            uri = parsed;
            return true;
        }

        uri = null!;
        return false;
    }
}
