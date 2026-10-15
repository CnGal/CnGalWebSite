using System.Text.Json.Serialization;

namespace CnGalWebSite.DrawingBed.Models.Sorting;

/// <summary>Supported sort keys for this list.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<DrawingBedUploadRecordSortField>))]
public enum DrawingBedUploadRecordSortField
{
    Id,
    Duration,
    Sha1,
    Size,
    Type,
    UploadTime,
    Url,
    UserId,
}
