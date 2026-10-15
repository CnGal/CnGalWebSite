using System.Text.Json.Serialization;

namespace CnGalWebSite.IdentityServer.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<IdentityClientSortField>))]
public enum IdentityClientSortField
{
    Id,
    ClientId,
    ClientName,
    ClientUri,
    Description,
    Enabled,
    LogoUri,
}
