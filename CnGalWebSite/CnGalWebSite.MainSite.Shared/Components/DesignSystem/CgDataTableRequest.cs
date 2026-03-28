namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// 数据表格服务端查询请求参数。
/// </summary>
public sealed record CgDataTableRequest(
    int Page,
    int PageSize,
    string? SortField,
    bool SortDesc,
    string? SearchText);
