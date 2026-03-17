using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ITagQueryService
{
    Task<SdkResult<List<TagTreeModel>>> GetTagTreeAsync(CancellationToken cancellationToken = default);

    Task<SdkResult<TagIndexViewModel>> GetTagDetailAsync(int id, CancellationToken cancellationToken = default);
}
