using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GameRecordSortField>))]
public enum GameRecordSortField
{
    Id,
    GameId,
    GameName,
    IsHidden,
    LastEditTime,
    PlayImpressions,
    ShowPublicly,
    TotalSocre,
    Type,
    UserId,
    UserName,
}
