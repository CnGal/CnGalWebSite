using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<StoreInfoSortField>))]
public enum StoreInfoSortField
{
    Id,
    CutNow,
    EntryId,
    Link,
    Name,
    PlatformName,
    PlatformType,
    PriceNow,
    State,
    UpdateTime,
    UpdateType,
}
