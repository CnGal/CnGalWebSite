using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExpoTaskSortField>))]
public enum ExpoTaskSortField
{
    Id,
    LotteryCount,
    Time,
    Type,
    UserId,
    UserName,
}
