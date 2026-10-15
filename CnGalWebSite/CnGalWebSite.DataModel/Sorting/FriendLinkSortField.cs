using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FriendLinkSortField>))]
public enum FriendLinkSortField
{
    Id,
    Image,
    Link,
    Name,
    Priority,
}
