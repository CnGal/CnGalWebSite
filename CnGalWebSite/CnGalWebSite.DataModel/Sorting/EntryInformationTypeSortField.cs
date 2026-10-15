using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<EntryInformationTypeSortField>))]
public enum EntryInformationTypeSortField
{
    Id,
    Description,
    Icon,
    IsHidden,
    Name,
}
