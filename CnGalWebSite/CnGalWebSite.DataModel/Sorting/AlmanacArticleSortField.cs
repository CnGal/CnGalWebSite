using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<AlmanacArticleSortField>))]
public enum AlmanacArticleSortField
{
    Id,
    ArticleId,
    ArticleName,
    Hide,
    Image,
    Priority,
}
