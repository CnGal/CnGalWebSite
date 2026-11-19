using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.SDK.MainSite.Models.Comments;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed record EntryPageModel(
    EntryDetailViewModel Entry,
    SdkResult<PlayedGameOverviewViewModel>? Score,
    SdkResult<GameOverviewPeripheryListModel> Periphery,
    SdkResult<IReadOnlyList<FavoriteFolderOverviewModel>> Catalog,
    SdkResult<EntryEditRecordPreview> EditRecords,
    SdkResult<CommentListViewModel> Comments);
