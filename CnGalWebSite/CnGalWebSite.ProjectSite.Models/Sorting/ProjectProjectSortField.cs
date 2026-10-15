using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectProjectSortField>))]
public enum ProjectProjectSortField
{
    Id,
    CreateTime,
    EndTime,
    Hide,
    Name,
    Priority,
    UserId,
    UserName,
}
