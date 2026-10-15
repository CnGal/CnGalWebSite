using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectCommentSortField>))]
public enum ProjectCommentSortField
{
    Id,
    CreateTime,
    Hide,
    PageId,
    PageType,
    Priority,
    Text,
    Type,
    UserId,
    UserName,
}
