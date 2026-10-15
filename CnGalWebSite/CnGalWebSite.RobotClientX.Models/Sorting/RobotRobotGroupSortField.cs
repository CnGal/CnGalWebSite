using System.Text.Json.Serialization;

namespace CnGalWebSite.RobotClientX.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RobotRobotGroupSortField>))]
public enum RobotRobotGroupSortField
{
    Id,
    ForceMatch,
    GroupId,
    IsHidden,
    Note,
}
