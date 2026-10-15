using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<StaffSortField>))]
public enum StaffSortField
{
    Id,
    CustomName,
    Modifier,
    Name,
    PositionGeneral,
    PositionOfficial,
    SubordinateOrganization,
}
