using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CarouselSortField>))]
public enum CarouselSortField
{
    Id,
    Image,
    Link,
    Note,
    Priority,
    Type,
}
