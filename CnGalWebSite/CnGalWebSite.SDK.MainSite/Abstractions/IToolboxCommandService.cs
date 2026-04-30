using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Toolbox;

namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IToolboxCommandService
{
    Task<SdkResult<ToolboxBilibiliVideoInfo>> GetBilibiliVideoInfoAsync(string id, CancellationToken cancellationToken = default);
    Task<SdkResult<IReadOnlyList<string>>> GetBilibiliVideoImageImpositionAsync(string id, CancellationToken cancellationToken = default);

    Task<SdkResult<int>> GetEntryIdAsync(string name, CancellationToken cancellationToken = default);
    Task<SdkResult<EntryIndexViewModel>> GetEntryViewAsync(int id, CancellationToken cancellationToken = default);
    Task<SdkResult<EditRelevancesViewModel>> GetEntryRelevancesAsync(int id, CancellationToken cancellationToken = default);
    Task<SdkResult<EditAddInforViewModel>> GetEntryAddInforAsync(int id, CancellationToken cancellationToken = default);
    Task<SdkResult<EditMainViewModel>> GetEntryMainAsync(int id, CancellationToken cancellationToken = default);
    Task<SdkResult<EditArticleRelevancesViewModel>> GetArticleRelevancesAsync(long id, CancellationToken cancellationToken = default);
    Task<SdkResult<EditVideoRelevancesViewModel>> GetVideoRelevancesAsync(long id, CancellationToken cancellationToken = default);

    Task<SdkResult<bool>> SubmitEntryRelevancesAsync(EditRelevancesViewModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> SubmitEntryAddInforAsync(EditAddInforViewModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> SubmitEntryMainAsync(EditMainViewModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> SubmitArticleRelevancesAsync(EditArticleRelevancesViewModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> SubmitVideoRelevancesAsync(EditVideoRelevancesViewModel model, CancellationToken cancellationToken = default);
    Task<SdkResult<bool>> HideEntryAsync(int id, CancellationToken cancellationToken = default);
}
