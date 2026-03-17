using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.TagEdit;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ITagCommandService
{
    Task<SdkResult<TagEditViewModel>> GetTagCreateTemplateAsync(CancellationToken cancellationToken = default);

    Task<SdkResult<TagEditViewModel>> GetTagEditAsync(int id, CancellationToken cancellationToken = default);

    Task<SdkResult<TagEditMetaOptions>> GetTagEditMetaOptionsAsync(CancellationToken cancellationToken = default);

    Task<SdkResult<long>> SubmitEditAsync(TagEditRequest request, CancellationToken cancellationToken = default);
}
