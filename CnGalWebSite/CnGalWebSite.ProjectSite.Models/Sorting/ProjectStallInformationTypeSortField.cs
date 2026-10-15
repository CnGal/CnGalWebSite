using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectStallInformationTypeSortField>))]
public enum ProjectStallInformationTypeSortField
{
    Id,
    Description,
    Hide,
    HideInfoCard,
    Icon,
    Name,
    Priority,
}
