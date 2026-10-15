using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<BasicInforSortField>))]
public enum BasicInforSortField
{
    Id,
    Engine,
    GameNickname,
    GamePlatforms,
    IssueMethod,
    IssueTime,
    Name,
    OfficialWebsite,
    Original,
    ProductionGroup,
    Publisher,
    QQgroupGame,
    RealId,
    SteamId,
    Tags,
}
