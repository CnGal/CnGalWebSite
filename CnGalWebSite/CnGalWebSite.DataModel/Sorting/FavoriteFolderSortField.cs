using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<FavoriteFolderSortField>))]
public enum FavoriteFolderSortField
{
    Id,
    BriefIntroduction,
    Count,
    CreateTime,
    IsDefault,
    IsHidden,
    MainImage,
    Name,
    ShowPublicly,
    UserId,
    UserName,
}
