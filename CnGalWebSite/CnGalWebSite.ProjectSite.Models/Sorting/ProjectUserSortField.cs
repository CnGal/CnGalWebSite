using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectUserSortField>))]
public enum ProjectUserSortField
{
    Id,
    Avatar,
    BackgroundImage,
    Email,
    Name,
    RegistTime,
    Tags,
    Type,
}
