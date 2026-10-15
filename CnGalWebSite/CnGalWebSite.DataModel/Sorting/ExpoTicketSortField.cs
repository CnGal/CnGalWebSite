using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExpoTicketSortField>))]
public enum ExpoTicketSortField
{
    Id,
    ActivityId,
    ActivityName,
    CreateTime,
    Hidden,
    Image,
    Nickname,
    Number,
    SeatInfo,
}
