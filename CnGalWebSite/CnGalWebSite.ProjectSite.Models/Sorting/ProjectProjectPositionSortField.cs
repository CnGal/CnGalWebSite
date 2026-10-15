using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectProjectPositionSortField>))]
public enum ProjectProjectPositionSortField
{
    Id,
    CreateTime,
    DeadLine,
    Description,
    Hide,
    PositionType,
    PositionTypeName,
    Priority,
    ProjectId,
    ProjectName,
    Tags,
    Type,
}
