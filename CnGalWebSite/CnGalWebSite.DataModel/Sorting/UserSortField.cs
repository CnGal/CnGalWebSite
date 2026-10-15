using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<UserSortField>))]
public enum UserSortField
{
    Id,
    CanComment,
    DisplayContributionValue,
    DisplayIntegral,
    Email,
    LastOnlineTime,
    OnlineTime,
    PersonalSignature,
    RegistTime,
    UserName,
}
