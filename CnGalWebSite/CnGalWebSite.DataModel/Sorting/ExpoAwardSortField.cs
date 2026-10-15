using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExpoAwardSortField>))]
public enum ExpoAwardSortField
{
    Id,
    Count,
    Image,
    IsEnabled,
    Name,
    PrizeCount,
    Type,
    Url,
}
