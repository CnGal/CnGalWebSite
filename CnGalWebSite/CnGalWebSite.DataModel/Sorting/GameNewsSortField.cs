using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GameNewsSortField>))]
public enum GameNewsSortField
{
    Id,
    ArticleId,
    Author,
    BriefIntroduction,
    PublishTime,
    RssType,
    State,
    Title,
}
