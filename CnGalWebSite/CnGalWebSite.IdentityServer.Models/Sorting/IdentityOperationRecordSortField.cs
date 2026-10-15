using System.Text.Json.Serialization;

namespace CnGalWebSite.IdentityServer.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<IdentityOperationRecordSortField>))]
public enum IdentityOperationRecordSortField
{
    Id,
    Ip,
    Time,
    Type,
    UserName,
}
