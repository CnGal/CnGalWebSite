using CnGalWebSite.Core.Models;

namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

public sealed record CgDataTableRequest<TSort>(int Page, int PageSize, TSort? SortField,
    bool SortDesc, string? SearchText) where TSort : struct, Enum
{
    public QueryParameterModel<TSort> ToQueryParameter() => new()
    {
        Page = Page, ItemsPerPage = PageSize, SearchText = SearchText ?? string.Empty,
        Sorts = SortField.HasValue
            ? [new(SortField.Value, SortDesc ? SortDirection.Descending : SortDirection.Ascending)] : []
    };
}
