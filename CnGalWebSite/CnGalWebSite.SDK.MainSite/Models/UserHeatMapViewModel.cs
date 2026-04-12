namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 热力图数据点：日期 + 计数值。
/// </summary>
public sealed class HeatMapDataPoint
{
    public required string Date { get; init; }
    public required int Count { get; init; }
}

/// <summary>
/// 用户热力图 ViewModel。
/// </summary>
public sealed class UserHeatMapViewModel
{
    public required IReadOnlyList<HeatMapDataPoint> Data { get; init; }
    public required int MaxCount { get; init; }
}
