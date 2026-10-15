using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MessageSortField>))]
public enum MessageSortField
{
    Id,
    IsReaded,
    Link,
    PostTime,
    Text,
    Title,
    Type,
    UserId,
    UserName,
}
