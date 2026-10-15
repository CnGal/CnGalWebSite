using System.Text.Json.Serialization;

namespace CnGalWebSite.ProjectSite.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ProjectOperationRecordSortField>))]
public enum ProjectOperationRecordSortField
{
    Id,
    Cookie,
    IP,
    PageId,
    PageType,
    Time,
    Type,
    UserId,
    UserName,
}
