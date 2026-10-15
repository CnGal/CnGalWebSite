using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RecommendSortField>))]
public enum RecommendSortField
{
    Id,
    EntryId,
    EntryName,
    IsHidden,
    Reason,
    UpdateTime,
}
