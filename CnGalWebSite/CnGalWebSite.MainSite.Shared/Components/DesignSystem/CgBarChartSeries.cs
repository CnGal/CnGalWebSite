namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// CgBarChart 的数据系列。
/// </summary>
public sealed class CgBarChartSeries
{
    /// <summary>
    /// 系列名称（用于图例）。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 数据点列表，与 Labels 一一对应。
    /// </summary>
    public IReadOnlyList<double> Data { get; init; } = [];

    /// <summary>
    /// CSS 颜色值（如 "#6366f1"），为空时使用内置调色板。
    /// </summary>
    public string? Color { get; init; }
}
