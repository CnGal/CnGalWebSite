using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IEntryQueryService
{
    Task<SdkResult<EntryDetailViewModel>> GetEntryDetailAsync(int id, CancellationToken cancellationToken = default);
}
