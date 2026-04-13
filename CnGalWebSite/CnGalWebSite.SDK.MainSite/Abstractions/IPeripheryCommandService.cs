using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.PeripheryEdit;
using CnGalWebSite.DataModel.ViewModel.Peripheries;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IPeripheryCommandService
{
    Task<SdkResult<PeripheryEditViewModel>> GetPeripheryCreateTemplateAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<PeripheryEditViewModel>> GetPeripheryEditAsync(long id, CancellationToken cancellationToken = default);
    Task<SdkResult<PeripheryEditMetaOptions>> GetPeripheryEditMetaOptionsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<long>> SubmitEditAsync(PeripheryEditRequest request, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> CheckIsCollectedAsync(long peripheryId, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> CollectAsync(long peripheryId, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> UnCollectAsync(long peripheryId, CancellationToken cancellationToken = default);
}
