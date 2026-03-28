using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.PeripheryEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IPeripheryCommandService
{
    Task<SdkResult<PeripheryEditViewModel>> GetPeripheryCreateTemplateAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<PeripheryEditViewModel>> GetPeripheryEditAsync(long id, CancellationToken cancellationToken = default);
    Task<SdkResult<PeripheryEditMetaOptions>> GetPeripheryEditMetaOptionsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<long>> SubmitEditAsync(PeripheryEditRequest request, CancellationToken cancellationToken = default);
}
