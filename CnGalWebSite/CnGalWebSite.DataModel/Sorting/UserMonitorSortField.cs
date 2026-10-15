using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<UserMonitorSortField>))]
public enum UserMonitorSortField
{
    Id,
    CreateTime,
    EntryId,
    EntryName,
    Type,
}
