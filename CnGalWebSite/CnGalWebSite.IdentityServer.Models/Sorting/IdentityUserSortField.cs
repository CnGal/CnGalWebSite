using System.Text.Json.Serialization;

namespace CnGalWebSite.IdentityServer.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<IdentityUserSortField>))]
public enum IdentityUserSortField
{
    Id,
    Email,
    UserName,
}
