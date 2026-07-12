using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IExpoQueryService
{
    Task<SdkResult<ExpoUserTaskModel>> GetUserTaskAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<List<ExpoAwardOverviewModel>>> GetAllAwardsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<List<ExpoPrizeOverviewModel>>> GetUserPrizesAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> CheckUserGameRatingAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> CheckUserBindQQAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> CheckUserChangeAvatarAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> CheckUserChangeSignatureAsync(CancellationToken cancellationToken = default);
}
