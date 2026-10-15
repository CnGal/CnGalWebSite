using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectCarouselSortField>))]
public enum ProjectCarouselSortField
{
    Id,
    Image,
    Link,
    Note,
    Priority,
}
