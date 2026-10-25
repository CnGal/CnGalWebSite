using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IEntryPageQueryService
{
    Task<SdkResult<EntryPageModel>> GetAsync(int id, CancellationToken cancellationToken = default);
}
