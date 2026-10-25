using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Favorites;
using CnGalWebSite.DataModel.ViewModel.Peripheries;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed record EntryPageModel(
    EntryDetailViewModel Entry,
    SdkResult<PlayedGameOverviewViewModel>? Score,
    SdkResult<GameOverviewPeripheryListModel> Periphery,
    SdkResult<IReadOnlyList<FavoriteFolderOverviewModel>> Catalog,
    SdkResult<IReadOnlyList<ExaminedNormalListModel>> EditRecords);
