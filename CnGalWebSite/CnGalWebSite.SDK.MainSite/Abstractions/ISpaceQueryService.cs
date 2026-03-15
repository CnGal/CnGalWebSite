using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ISpaceQueryService
{
    Task<SdkResult<SpaceDetailViewModel>> GetSpaceDetailAsync(string id, CancellationToken cancellationToken = default);
}
