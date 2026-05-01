namespace CnGalWebSite.SDK.MainSite.Models.Files;

/// <summary>
/// 图片上传成功后返回的结果
/// </summary>
public sealed class ImageUploadResult
{
    public required string Url { get; init; }

    public string? FileName { get; init; }
}
