using System.Text.Json.Serialization;

namespace CnGalWebSite.RobotClientX.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RobotRobotEventSortField>))]
public enum RobotRobotEventSortField
{
    Id,
    DelaySecond,
    IsHidden,
    Note,
    Probability,
    Text,
    Time,
    Type,
}
