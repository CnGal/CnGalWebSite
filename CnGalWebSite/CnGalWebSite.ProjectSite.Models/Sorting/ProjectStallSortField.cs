using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectStallSortField>))]
public enum ProjectStallSortField
{
    Id,
    CreateTime,
    EndTime,
    Hide,
    Name,
    Price,
    Priority,
    UserId,
    UserName,
}
