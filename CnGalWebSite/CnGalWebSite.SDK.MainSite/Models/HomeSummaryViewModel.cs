using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tables;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class HomeSummaryViewModel
{
    public required string HeroTitle { get; init; }

    public required string HeroSubtitle { get; init; }

    public required IReadOnlyList<CarouselViewModel> Carousels { get; init; }

    public required IReadOnlyList<HotRecommendItemModel> HotRecommends { get; set; }

    public required IReadOnlyList<PublishedGameItemModel> PublishedGames { get; init; }

    public required IReadOnlyList<RecentlyDemoGameItemModel> RecentlyDemoGames { get; init; }

    public required IReadOnlyList<UpcomingGameItemModel> UpcomingGames { get; init; }

    public required IReadOnlyList<CarouselViewModel> ActivityCarousels { get; init; }

    public required IReadOnlyList<HomeNewsAloneViewModel> HomeNews { get; init; }

    public required IReadOnlyList<ArticleInforTipViewModel> WeeklyNews { get; init; }

    public required IReadOnlyList<LatestArticleItemModel> LatestArticles { get; init; }

    public required IReadOnlyList<LatestVideoItemModel> LatestVideos { get; init; }

    public required IReadOnlyList<FriendLinkItemModel> FriendLinks { get; init; }

    public required IReadOnlyList<RoleBrithdayViewModel> Birthdays { get; init; }

    public required IReadOnlyList<AnnouncementItemModel> Announcements { get; init; }

    public required IReadOnlyList<RecentlyEditedGameItemModel> RecentlyEditedGames { get; init; }

    public required IReadOnlyList<HomeCommunityLinkViewModel> CommunityLinks { get; init; }

    public required IReadOnlyList<EvaluationItemModel> Evaluations { get; set; }

    public required HomeSupportLinkViewModel? SupportLink { get; init; }

    public required IReadOnlyList<HotTagItemModel> HotTags { get; init; }

    public required IReadOnlyList<LatestCommentItemModel> LatestComments { get; init; }

    public required IReadOnlyList<FreeGameItemModel> FreeGames { get; init; }

    public required IReadOnlyList<DiscountGameItemModel> DiscountGames { get; init; }

    public required TableViewModel CommunityStats { get; init; }

    public required IReadOnlyList<SdkErrorModel> Warnings { get; init; }
}

public sealed class HomeCommunityLinkViewModel
{
    public required string Title { get; init; }

    public required string Text { get; init; }

    public required string Link { get; init; }
}

public sealed class HomeSupportLinkViewModel
{
    public required string Title { get; init; }

    public required string Text { get; init; }

    public required string Link { get; init; }
}
