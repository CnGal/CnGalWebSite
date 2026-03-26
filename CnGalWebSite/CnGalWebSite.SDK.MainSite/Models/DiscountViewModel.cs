namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 折扣页面使用的游戏条目 ViewModel
/// </summary>
public sealed class DiscountGameItem
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string BriefIntroduction { get; init; } = "";
    public string MainImage { get; init; } = "";
    public DateTime? PublishTime { get; init; }
    public double OriginalPrice { get; init; }
    public double PriceNow { get; init; }
    public double CutNow { get; init; }
    public double PriceLowest { get; init; }
    public double CutLowest { get; init; }
    public int EvaluationCount { get; init; }
    public double RecommendationRate { get; init; }
    public int PlayTime { get; init; }
}
