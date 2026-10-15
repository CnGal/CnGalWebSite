using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<OperationRecordSortField>))]
public enum OperationRecordSortField
{
    Id,
    Cookie,
    Ip,
    ObjectId,
    OperationTime,
    Type,
    UserId,
    UserName,
}
