using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Files;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IFileCommandService
{
    Task<SdkResult<string>> UploadImageAsync(
        Stream fileStream,
        string fileName,
        ImageAspectType aspectType = ImageAspectType.None,
        bool gallery = false,
        ImageCropRect? cropRect = null,
        CancellationToken cancellationToken = default);

    Task<SdkResult<AudioUploadResult>> UploadAudioAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<SdkResult<string>> TransformImageUrlAsync(
        string url,
        double x = 0,
        double y = 0,
        CancellationToken cancellationToken = default);
}
