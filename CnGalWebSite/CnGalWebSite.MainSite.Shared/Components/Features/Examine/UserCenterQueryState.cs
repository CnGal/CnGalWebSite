using CnGalWebSite.MainSite.Shared.Components.DesignSystem;

namespace CnGalWebSite.MainSite.Shared.Components.Features.Examine;

/// <summary>内容中心查询参数的容错解析。</summary>
public static class UserCenterQueryState
{
    public static string GetTab(string? tab) => tab is "examines" or "reviews" or "monitors" ? tab : "overview";

    public static int GetPage(string? page) => int.TryParse(page, out var value) && value > 0 ? value : 1;

    public static CgDataTableRequest<TSort> GetTableState<TSort>(
        string? page, string? pageSize, string? search, string? sort, string? desc,
        TSort defaultSort, params TSort[] sortableFields) where TSort : struct, Enum
    {
        var size = int.TryParse(pageSize, out var value) && value is 5 or 10 or 20 or 50 ? value : 10;
        var field = Enum.TryParse<TSort>(sort, out var parsed) && sortableFields.Contains(parsed)
            ? parsed : defaultSort;
        return new(GetPage(page), size, field,
            !bool.TryParse(desc, out var descending) || descending,
            string.IsNullOrWhiteSpace(search) ? null : search);
    }
}
