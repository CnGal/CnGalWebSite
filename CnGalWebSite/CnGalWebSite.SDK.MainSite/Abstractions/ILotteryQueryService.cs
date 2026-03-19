using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ILotteryQueryService
{
    Task<SdkResult<LotteryDetailViewModel>> GetLotteryDetailAsync(long id, CancellationToken cancellationToken = default);

    Task<SdkResult<IReadOnlyList<LotteryCardItemModel>>> GetLotteryCardsAsync(CancellationToken cancellationToken = default);
}
