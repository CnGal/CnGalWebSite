using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Expo;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IExpoCommandService
{
    Task<SdkResult<bool>> FinishTaskAsync(ExpoTaskType type, CancellationToken cancellationToken = default);
    Task<SdkResult<long?>> LotteryAsync(CancellationToken cancellationToken = default);
}
