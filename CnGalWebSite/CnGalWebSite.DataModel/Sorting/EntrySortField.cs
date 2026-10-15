using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<EntrySortField>))]
public enum EntrySortField
{
    Id,
    CanComment,
    IsHidden,
    IsHideOutlink,
    Name,
    Priority,
    Type,
}
