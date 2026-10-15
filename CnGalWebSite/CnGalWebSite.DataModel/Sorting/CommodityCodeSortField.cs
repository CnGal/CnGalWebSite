using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CommodityCodeSortField>))]
public enum CommodityCodeSortField
{
    Id,
    CanRedeemed,
    Code,
    Count,
    CreateTime,
    Hide,
    Redeemed,
    RedeemedTime,
    Type,
    UpdateTime,
    UserId,
    UserName,
}
