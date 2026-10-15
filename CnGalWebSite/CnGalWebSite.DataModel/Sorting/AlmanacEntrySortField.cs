using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AlmanacEntrySortField>))]
public enum AlmanacEntrySortField
{
    Id,
    EntryId,
    EntryName,
    Hide,
    Image,
    Priority,
}
