using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<WeeklyNewsSortField>))]
public enum WeeklyNewsSortField
{
    Id,
    ArticleId,
    BriefIntroduction,
    CreateTime,
    PublishTime,
    State,
    Title,
}
