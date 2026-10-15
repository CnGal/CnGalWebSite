using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GCoinsRecordSortField>))]
public enum GCoinsRecordSortField
{
    Id,
    Count,
    Note,
    SourceType,
    Time,
}
