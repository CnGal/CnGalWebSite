using System.Text.Json.Serialization;

namespace CnGalWebSite.IdentityServer.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<IdentityExamineSortField>))]
public enum IdentityExamineSortField
{
    Id,
    ApplyTime,
    IsPassed,
    PassedAdminName,
    PassedTime,
    Type,
    UserName,
}
