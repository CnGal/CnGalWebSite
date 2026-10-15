using System;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// 数据表格列定义组件。不渲染任何 UI，仅向父 <see cref="CgDataTable{TItem, TSort}"/> 注册列元数据。
/// </summary>
/// <typeparam name="TItem">行数据类型。</typeparam>
public class CgDataTableColumn<TItem, TSort> : ComponentBase, IDisposable where TSort : struct, Enum
{
    [CascadingParameter]
    public CgDataTable<TItem, TSort>? Parent { get; set; }

    /// <summary>
    /// 列标题。
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通过类型化选择器读取行数据。
    /// 为空时该列不可排序。
    /// </summary>
    [Parameter]
    public Func<TItem, object?>? Value { get; set; }

    [Parameter]
    public TSort? SortField { get; set; }

    /// <summary>
    /// 是否可排序。默认 true（需同时设置 <see cref="SortField"/>）。
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// 自定义单元格模板。context 为当前行的 <typeparamref name="TItem"/>。
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? ChildContent { get; set; }

    /// <summary>
    /// 列宽度（CSS 值），如 "120px"、"20%"。
    /// </summary>
    [Parameter]
    public string? Width { get; set; }

    /// <summary>
    /// 该列是否真正可参与排序。需要同时满足 Sortable=true 且 SortField 和 Value 均已设置。
    /// </summary>
    public bool CanSort => Sortable && SortField.HasValue && Value != null;

    protected override void OnInitialized()
    {
        Parent?.AddColumn(this);
    }

    public void Dispose()
    {
        Parent?.RemoveColumn(this);
    }
}
