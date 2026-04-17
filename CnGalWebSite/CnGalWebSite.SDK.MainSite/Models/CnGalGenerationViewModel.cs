namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// "CnGal 世代" 按年分组的游戏列表项（只读 DTO/ViewModel，UI 状态由页面自行维护）。
/// </summary>
public sealed class CnGalGenerationYearItem
{
    public required int Year { get; init; }

    public required IReadOnlyList<CnGalGenerationGameItem> Games { get; init; }
}

/// <summary>
/// "CnGal 世代" 单个游戏项。
/// </summary>
public sealed class CnGalGenerationGameItem
{
    public required string Name { get; init; }
}
