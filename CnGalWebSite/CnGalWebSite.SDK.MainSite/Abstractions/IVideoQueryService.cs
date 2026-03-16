using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IVideoQueryService
{
    Task<SdkResult<VideoDetailViewModel>> GetVideoDetailAsync(long id, CancellationToken cancellationToken = default);
}
