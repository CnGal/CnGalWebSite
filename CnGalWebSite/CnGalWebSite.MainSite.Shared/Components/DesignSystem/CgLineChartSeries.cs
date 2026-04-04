namespace CnGalWebSite.MainSite.Shared.Components.DesignSystem;

/// <summary>
/// CgLineChart 的数据系列。
/// </summary>
public sealed class CgLineChartSeries
{
    /// <summary>
    /// 系列名称（用于图例）。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 数据点列表，与 Labels 一一对应。
    /// </summary>
    public IReadOnlyList<double> Data { get; init; } = [];
}
