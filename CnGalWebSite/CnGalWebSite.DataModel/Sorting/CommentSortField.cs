using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CommentSortField>))]
public enum CommentSortField
{
    Id,
    CommentTime,
    IsHidden,
    ObjectName,
    Priority,
    Text,
    Type,
    UserId,
    UserName,
}
