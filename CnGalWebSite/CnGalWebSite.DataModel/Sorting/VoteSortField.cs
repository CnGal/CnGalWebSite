using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<VoteSortField>))]
public enum VoteSortField
{
    Id,
    BeginTime,
    CanComment,
    EndTime,
    IsHidden,
    LastEditTime,
    Name,
    Priority,
    Type,
}
