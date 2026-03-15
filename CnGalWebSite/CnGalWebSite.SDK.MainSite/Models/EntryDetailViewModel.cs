using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class EntryDetailViewModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string AnotherName { get; init; }

    public required string BriefIntroduction { get; init; }

    public required EntryType Type { get; init; }

    public required string MainPicture { get; init; }

    public required string Thumbnail { get; init; }

    public required string BackgroundPicture { get; init; }

    public required string SmallBackgroundPicture { get; init; }

    public required bool IsEdit { get; init; }

    public required bool IsHidden { get; init; }

    public required bool IsHideOutlink { get; init; }

    public required bool IsScored { get; init; }

    public required bool CanComment { get; init; }

    public required int TabIndex { get; init; }

    public required string MainPage { get; init; }

    public required BookingViewModel? Booking { get; init; }

    public required IReadOnlyList<EntryInformationModel> Information { get; init; }

    public required IReadOnlyList<string> Tags { get; init; }

    public required IReadOnlyList<EditAudioAloneModel> Audio { get; init; }

    public required IReadOnlyList<PicturesAloneViewModel> Pictures { get; init; }

    public required IReadOnlyList<NewsModel> NewsOfEntry { get; init; }

    public required IReadOnlyList<EntryInforTipViewModel> EntryRelevances { get; init; }

    public required IReadOnlyList<ArticleInforTipViewModel> ArticleRelevances { get; init; }

    public required IReadOnlyList<VideoInforTipViewModel> VideoRelevances { get; init; }

    public required IReadOnlyList<RelevancesKeyValueModel> OtherRelevances { get; init; }

    public required IReadOnlyList<EntryRoleViewModel> Roles { get; init; }

    public required IReadOnlyList<EntryInforTipViewModel> StaffGames { get; init; }

    public required IReadOnlyList<StaffInforModel> Staffs { get; init; }

    public required IReadOnlyList<StaffNameModel> ProductionGroups { get; init; }

    public required IReadOnlyList<StaffNameModel> Publishers { get; init; }

    public required IReadOnlyList<GameReleaseViewModel> Releases { get; init; }
}
