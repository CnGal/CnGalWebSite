namespace CnGalWebSite.SDK.MainSite.Models.Files;

/// <summary>
/// 音频上传成功后返回的结果
/// </summary>
public sealed class AudioUploadResult
{
    public required string Url { get; init; }

    public string? FileName { get; init; }

    public TimeSpan Duration { get; init; }
}
