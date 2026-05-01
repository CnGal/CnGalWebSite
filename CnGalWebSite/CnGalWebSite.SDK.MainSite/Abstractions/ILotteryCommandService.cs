using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Lotteries;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ILotteryCommandService
{
    Task<SdkResult<IReadOnlyList<string>>> GetEntryGameItemsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<EditLotteryModel>> GetLotteryEditAsync(long id, CancellationToken cancellationToken = default);
    Task<SdkResult<long>> CreateLotteryAsync(EditLotteryModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<long>> EditLotteryAsync(EditLotteryModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<UserLotteryStateModel>> GetUserLotteryStateAsync(long lotteryId, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> ParticipateInLotteryAsync(long lotteryId, DeviceIdentificationModel identification, CancellationToken cancellationToken = default);
}
