using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models.Files;
using Microsoft.AspNetCore.Components.Forms;

namespace CnGalWebSite.MainSite.Shared;

public static class FileCommandServiceExtensions
{
    private const long MaxImageUploadBytes = 10 * 1024 * 1024;
    private const long MaxAudioUploadBytes = 50 * 1024 * 1024;

    public static async Task<ImageUploadResult?> UploadImageAsync(
        this IFileCommandService service,
        IBrowserFile file,
        CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream(MaxImageUploadBytes);
        var result = await service.UploadImageAsync(stream, file.Name, cancellationToken: ct);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            return null;
        }

        return new ImageUploadResult { Url = result.Data, FileName = file.Name };
    }

    public static async Task<AudioUploadResult?> UploadAudioAsync(
        this IFileCommandService service,
        IBrowserFile file,
        CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream(MaxAudioUploadBytes);
        var result = await service.UploadAudioAsync(stream, file.Name, ct);
        if (!result.Success || result.Data is null)
        {
            return null;
        }

        return result.Data;
    }
}
