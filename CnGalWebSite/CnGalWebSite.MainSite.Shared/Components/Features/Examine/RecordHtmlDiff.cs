namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

public static class RecordHtmlDiff
{
    public static string Build(string? before, string? after)
    {
        before ??= string.Empty;
        after ??= string.Empty;
        if (string.Equals(before, after, StringComparison.Ordinal)) return after;
        if (string.IsNullOrWhiteSpace(before)) return $"<ins>{after}</ins>";
        if (string.IsNullOrWhiteSpace(after)) return $"<del>{before}</del>";
        return new HtmlDiff.HtmlDiff(before, after).Build();
    }
}
