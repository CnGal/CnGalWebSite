using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<UserCertificationSortField>))]
public enum UserCertificationSortField
{
    Id,
    CertificationTime,
    EntryId,
    EntryName,
    EntryType,
    UserId,
    UserName,
}
