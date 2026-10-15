using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExpoPrizeSortField>))]
public enum ExpoPrizeSortField
{
    Id,
    AwardId,
    AwardName,
    AwardType,
    Content,
    DrawTime,
    UserId,
    UserName,
}
