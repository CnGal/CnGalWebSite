using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ExamineSortField>))]
public enum ExamineSortField
{
    Id,
    ApplicationUserId,
    ApplyTime,
    Comments,
    IsPassed,
    Operation,
    PassedAdminName,
    PassedTime,
    UserName,
}
