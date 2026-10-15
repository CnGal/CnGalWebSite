using System.Text.Json.Serialization;

namespace CnGalWebSite.IdentityServer.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<IdentityIdentityResourceSortField>))]
public enum IdentityIdentityResourceSortField
{
    Id,
    Description,
    DisplayName,
    Enabled,
    Name,
    Required,
}
