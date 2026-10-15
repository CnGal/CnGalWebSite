using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GroupInforSortField>))]
public enum GroupInforSortField
{
    Id,
    AnotherNameGroup,
    Bilibili,
    MicroBlog,
    Name,
    QQgroupGroup,
    RealId,
}
