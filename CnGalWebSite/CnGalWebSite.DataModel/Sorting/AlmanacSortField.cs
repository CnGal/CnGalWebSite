using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AlmanacSortField>))]
public enum AlmanacSortField
{
    Id,
    ArticleCount,
    BriefIntroduction,
    EntryCount,
    Name,
    UpdateTime,
    Year,
}
