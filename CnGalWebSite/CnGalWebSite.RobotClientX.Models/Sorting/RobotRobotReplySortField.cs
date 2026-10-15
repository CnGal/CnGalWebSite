using System.Text.Json.Serialization;

namespace CnGalWebSite.RobotClientX.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RobotRobotReplySortField>))]
public enum RobotRobotReplySortField
{
    Id,
    AfterTime,
    BeforeTime,
    IsHidden,
    Key,
    Priority,
    Range,
    UpdateTime,
    Value,
}
