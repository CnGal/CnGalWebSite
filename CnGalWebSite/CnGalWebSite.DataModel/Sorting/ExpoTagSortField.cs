using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExpoTagSortField>))]
public enum ExpoTagSortField
{
    Id,
    Hidden,
    Name,
    Priority,
}
