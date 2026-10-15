using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectFriendLinkSortField>))]
public enum ProjectFriendLinkSortField
{
    Id,
    Image,
    Link,
    Name,
    Priority,
}
