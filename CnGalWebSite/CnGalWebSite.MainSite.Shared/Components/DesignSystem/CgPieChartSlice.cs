namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// CgPieChart 的数据分片。
/// </summary>
public sealed class CgPieChartSlice
{
    /// <summary>
    /// 分片名称（用于图例）。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 分片数值。所有分片的 Value 总和用于计算百分比。
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// CSS 颜色值（如 "#ef4444" 或 "var(--cg-color-error)"），为空时使用内置调色板。
    /// </summary>
    public string? Color { get; init; }
}
