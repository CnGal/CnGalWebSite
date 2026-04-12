using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IVoteQueryService
{
    Task<SdkResult<IReadOnlyList<VoteCardItemModel>>> GetVoteCardsAsync(CancellationToken cancellationToken = default);
}
