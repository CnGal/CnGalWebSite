using System.Text.Json.Serialization;

namespace CnGalWebSite.RobotClientX.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RobotRobotFaceSortField>))]
public enum RobotRobotFaceSortField
{
    Id,
    IsHidden,
    Key,
    Note,
    Value,
}
