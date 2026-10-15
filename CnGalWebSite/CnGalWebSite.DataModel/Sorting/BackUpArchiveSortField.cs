using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<BackUpArchiveSortField>))]
public enum BackUpArchiveSortField
{
    Id,
    IsLastFail,
    LastBackUpTime,
    LastTimeUsed,
    ObjectId,
    ObjectName,
    Type,
}
