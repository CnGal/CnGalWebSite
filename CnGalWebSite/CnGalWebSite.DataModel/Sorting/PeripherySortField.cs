using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<PeripherySortField>))]
public enum PeripherySortField
{
    Id,
    CanComment,
    Category,
    IsHidden,
    LastEditTime,
    Name,
    Priority,
    Type,
}
