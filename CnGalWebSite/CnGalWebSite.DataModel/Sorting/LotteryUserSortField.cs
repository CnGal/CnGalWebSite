using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<LotteryUserSortField>))]
public enum LotteryUserSortField
{
    Id,
    Cookie,
    Ip,
    IsHidden,
    LotteryUserId,
    Name,
    Number,
    UserId,
}
