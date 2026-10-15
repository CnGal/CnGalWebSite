using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<LotterySortField>))]
public enum LotterySortField
{
    Id,
    BeginTime,
    CanComment,
    EndTime,
    IsEnd,
    IsHidden,
    LastEditTime,
    LotteryTime,
    Name,
    Priority,
    Type,
}
