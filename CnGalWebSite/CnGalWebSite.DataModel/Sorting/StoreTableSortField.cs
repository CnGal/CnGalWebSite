using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<StoreTableSortField>))]
public enum StoreTableSortField
{
    Id,
    CurrencyCode,
    CutLowest,
    CutNow,
    EstimationOwnersMax,
    EstimationOwnersMin,
    EvaluationCount,
    Link,
    Name,
    OriginalPrice,
    PlatformName,
    PlatformType,
    PlayTime,
    PriceLowest,
    PriceNow,
    RecommendationRate,
    State,
    UpdateTime,
    UpdateType,
}
