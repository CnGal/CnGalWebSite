using CnGalWebSite.DataModel.Model;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.EntryEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IEntryCommandService
{
    Task<SdkResult<EntryEditViewModel>> GetEntryCreateTemplateAsync(EntryType type = EntryType.Game, CancellationToken cancellationToken = default);
    Task<SdkResult<EntryEditViewModel>> GetEntryEditAsync(int id, CancellationToken cancellationToken = default);
    Task<SdkResult<EntryEditMetaOptions>> GetEntryEditMetaOptionsAsync(CancellationToken cancellationToken = default);
    Task<SdkResult<long>> SubmitEditAsync(EntryEditRequest request, CancellationToken cancellationToken = default);
}
