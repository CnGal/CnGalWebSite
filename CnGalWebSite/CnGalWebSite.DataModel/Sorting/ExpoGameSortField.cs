using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExpoGameSortField>))]
public enum ExpoGameSortField
{
    Id,
    GameId,
    GameName,
    Hidden,
    Priority,
}
