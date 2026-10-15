using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RankSortField>))]
public enum RankSortField
{
    Id,
    CSS,
    Count,
    CreateTime,
    Image,
    IsHidden,
    LastEditTime,
    Name,
    Priority,
    Styles,
    Text,
    Type,
}
