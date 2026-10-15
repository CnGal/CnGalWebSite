using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectMessageSortField>))]
public enum ProjectMessageSortField
{
    Id,
    CreateTime,
    Hide,
    PageId,
    PageType,
    Read,
    Text,
    Type,
    UserId,
    UserName,
}
