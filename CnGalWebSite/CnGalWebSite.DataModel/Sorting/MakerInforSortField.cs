using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<MakerInforSortField>))]
public enum MakerInforSortField
{
    Id,
    AnotherName,
    Bilibili,
    Blog,
    Lofter,
    MicroBlog,
    Name,
    Nickname,
    Other,
    RealId,
}
