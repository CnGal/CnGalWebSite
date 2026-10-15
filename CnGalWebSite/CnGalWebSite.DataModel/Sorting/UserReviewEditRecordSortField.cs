using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<UserReviewEditRecordSortField>))]
public enum UserReviewEditRecordSortField
{
    Id,
    EntryId,
    EntryName,
    ExamineId,
    Operation,
    ReviewedTime,
    State,
    UserId,
    UserName,
}
