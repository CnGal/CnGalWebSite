using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<VideoSortField>))]
public enum VideoSortField
{
    Id,
    CanComment,
    CreateTime,
    IsHidden,
    LastEditTime,
    Name,
    Priority,
    Type,
}
