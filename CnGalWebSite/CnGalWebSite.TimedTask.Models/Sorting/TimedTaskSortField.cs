using System.Text.Json.Serialization;

namespace CnGalWebSite.TimedTask.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TimedTaskSortField>))]
public enum TimedTaskSortField
{
    Id,
    EveryTime,
    ExecuteType,
    IntervalTime,
    IsLastFail,
    IsPause,
    IsRuning,
    LastExecutedTime,
    Parameter,
    Type,
}
