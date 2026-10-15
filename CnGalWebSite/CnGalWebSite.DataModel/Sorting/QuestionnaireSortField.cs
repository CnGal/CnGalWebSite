using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<QuestionnaireSortField>))]
public enum QuestionnaireSortField
{
    Id,
    BeginTime,
    CreateTime,
    DisplayName,
    EndTime,
    IsActive,
    IsHidden,
    LastEditTime,
    Name,
    Priority,
    ResponseCount,
}
