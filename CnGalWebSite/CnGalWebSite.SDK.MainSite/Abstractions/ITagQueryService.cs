using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.ThematicPages;
using CnGalWebSite.SDK.MainSite.Models;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface ITagQueryService
{
    Task<SdkResult<IReadOnlyList<TagTreeModel>>> GetTagTreeAsync(CancellationToken cancellationToken = default);

    Task<SdkResult<TagIndexViewModel>> GetTagDetailAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取 CV 专题页数据
    /// </summary>
    Task<SdkResult<CVThematicPageViewModel>> GetCVThematicPageAsync(CancellationToken cancellationToken = default);
}
