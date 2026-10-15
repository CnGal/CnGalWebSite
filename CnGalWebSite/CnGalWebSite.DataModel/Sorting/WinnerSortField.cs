using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<WinnerSortField>))]
public enum WinnerSortField
{
    Id,
    ActivationCode,
    Address,
    AwardId,
    AwardName,
    AwardType,
    Email,
    Number,
    Phone,
    PrizeId,
    RealName,
    TrackingNumber,
    UserName,
}
