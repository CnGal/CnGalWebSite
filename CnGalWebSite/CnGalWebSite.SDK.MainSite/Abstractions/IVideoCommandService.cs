using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.VideoEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IVideoCommandService
{
    Task<SdkResult<VideoEditViewModel>> GetVideoCreateTemplateAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<VideoEditViewModel>> GetVideoEditAsync(long id, CancellationToken cancellationToken = default);
    Task<SdkResult<VideoEditMetaOptions>> GetVideoEditMetaOptionsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<long>> SubmitEditAsync(VideoEditRequest request, CancellationToken cancellationToken = default);
}
