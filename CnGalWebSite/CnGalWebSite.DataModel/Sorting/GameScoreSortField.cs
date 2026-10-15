using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GameScoreSortField>))]
public enum GameScoreSortField
{
    Id,
    AllCVSocre,
    AllMusicSocre,
    AllPaintSocre,
    AllScriptSocre,
    AllShowSocre,
    AllSystemSocre,
    AllTotalSocre,
    FilterCVSocre,
    FilterMusicSocre,
    FilterPaintSocre,
    FilterScriptSocre,
    FilterShowSocre,
    FilterSystemSocre,
    FilterTotalSocre,
    GameId,
    GameName,
}
