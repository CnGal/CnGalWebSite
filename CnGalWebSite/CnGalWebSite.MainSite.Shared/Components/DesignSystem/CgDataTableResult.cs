using System.Collections.Generic;

namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// 数据表格服务端查询结果。
/// </summary>
public sealed record CgDataTableResult<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalCount);
