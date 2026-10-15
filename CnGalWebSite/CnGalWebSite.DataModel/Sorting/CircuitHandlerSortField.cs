using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<CircuitHandlerSortField>))]
public enum CircuitHandlerSortField
{
    Id,
    IsConnected,
    LastActiveTime,
    OpenTime,
    ReConnectCount,
}
