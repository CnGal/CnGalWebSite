using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FavoriteObjectSortField>))]
public enum FavoriteObjectSortField
{
    Id,
    CreateTime,
    ObjectId,
    ObjectName,
    Type,
}
