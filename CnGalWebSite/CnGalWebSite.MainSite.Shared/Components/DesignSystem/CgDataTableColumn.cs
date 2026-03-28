using System;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// 数据表格列定义组件。不渲染任何 UI，仅向父 <see cref="CgDataTable{TItem}"/> 注册列元数据。
/// </summary>
/// <typeparam name="TItem">行数据类型。</typeparam>
public class CgDataTableColumn<TItem> : ComponentBase, IDisposable
{
    [CascadingParameter]
    public CgDataTable<TItem>? Parent { get; set; }

    /// <summary>
    /// 列标题。
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 排序字段名（对应 <typeparamref name="TItem"/> 的属性名）。
    /// 为空时该列不可排序。
    /// </summary>
    [Parameter]
    public string? Field { get; set; }

    /// <summary>
    /// 是否可排序。默认 true（需同时设置 <see cref="Field"/>）。
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
    /// 该列是否真正可参与排序。需要同时满足 Sortable=true 且 Field 不为空。
    /// </summary>
    public bool CanSort => Sortable && !string.IsNullOrWhiteSpace(Field);

    protected override void OnInitialized()
    {
        Parent?.AddColumn(this);
    }

    public void Dispose()
    {
        Parent?.RemoveColumn(this);
    }
}
