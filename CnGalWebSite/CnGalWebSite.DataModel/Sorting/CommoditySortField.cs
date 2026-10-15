using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CommoditySortField>))]
public enum CommoditySortField
{
    Id,
    BriefIntroduction,
    Image,
    IsHidden,
    LastEditTime,
    Name,
    Price,
    Priority,
    Type,
    UserCount,
    Value,
}
