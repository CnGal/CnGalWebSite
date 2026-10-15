#nullable enable
using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Sorting;

namespace CnGalWebSite.Extensions;

/// <summary>Converts the public search page's historic Sort parameter at the frontend boundary.</summary>
public static class SearchPageUrl
{
    public static bool TryParseSort(string? value, out SearchSortField field, out SortDirection direction)
    {
        field = SearchSortField.Default;
        direction = SortDirection.Descending;
        var parts = value?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) ?? [];
        // The old sort chips could emit " desc" when selecting the default sort.
        if (parts.Length == 0 || (parts.Length == 1 && parts[0].Equals("desc", StringComparison.OrdinalIgnoreCase)))
            return true;
        if (parts.Length > 2) return false;

        SearchSortField? parsed = parts[0].ToLowerInvariant() switch
        {
            "id" => SearchSortField.Id,
            "lastedittime" => SearchSortField.LastEditTime,
            "pubulishtime" or "publishtime" => SearchSortField.PublishTime,
            "createtime" => SearchSortField.CreateTime,
            "readercount" => SearchSortField.ReaderCount,
            _ => null
        };
        if (!parsed.HasValue) return false;
        if (parts.Length == 2 && !parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) &&
            !parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
            return false;

        field = parsed.Value;
        direction = parts.Length == 2 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
            ? SortDirection.Descending : SortDirection.Ascending;
        return true;
    }

    public static string FormatSort(SearchSortField field, SortDirection direction)
    {
        if (!Enum.IsDefined(direction)) throw new ArgumentOutOfRangeException(nameof(direction));
        var name = field switch
        {
            SearchSortField.Default => string.Empty,
            SearchSortField.Id => "Id",
            SearchSortField.LastEditTime => "LastEditTime",
            // Preserve the spelling used in existing bookmarks and external links.
            SearchSortField.PublishTime => "PubulishTime",
            SearchSortField.CreateTime => "CreateTime",
            SearchSortField.ReaderCount => "ReaderCount",
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };
        return name.Length == 0 ? name : name + (direction == SortDirection.Descending ? " desc" : string.Empty);
    }

    public static string ToPageQueryString(this SearchInputModel request)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(request.FilterText))
            parts.Add("Text=" + Uri.EscapeDataString(request.FilterText));
        var sort = FormatSort(request.SortField, request.SortDirection);
        if (sort.Length != 0) parts.Add("Sort=" + Uri.EscapeDataString(sort));
        if (request.CurrentPage != 1) parts.Add("Page=" + request.CurrentPage);
        parts.AddRange(request.Types.Select(type => "Types=" + Uri.EscapeDataString(type.ToString())));
        parts.AddRange(request.Times.Select(time => "Times=" + Uri.EscapeDataString(time.ToString())));
        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}
