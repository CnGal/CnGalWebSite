using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RoleInforSortField>))]
public enum RoleInforSortField
{
    Id,
    AnotherNameRole,
    Birthday,
    BloodType,
    CV,
    Character,
    ClothesAccessories,
    FigureData,
    FigureSubjective,
    GameName,
    Gender,
    Haircolor,
    Name,
    Pupilcolor,
    RealId,
    RoleAge,
    RoleHeight,
    RoleIdentity,
    RoleTaste,
}
