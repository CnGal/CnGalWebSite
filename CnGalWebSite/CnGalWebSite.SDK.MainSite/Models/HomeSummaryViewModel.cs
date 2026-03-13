namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class HomeSummaryViewModel
{
    public required string HeroTitle { get; init; }

    public required string HeroSubtitle { get; init; }

    public required IReadOnlyList<HomeFeaturedEntryViewModel> FeaturedEntries { get; init; }

    public required IReadOnlyList<HomeCarouselViewModel> Carousels { get; init; }

    public required IReadOnlyList<HomeAnnouncementViewModel> Announcements { get; init; }

    public required IReadOnlyList<HomeArticleViewModel> LatestArticles { get; init; }

    public required IReadOnlyList<SdkErrorModel> Warnings { get; init; }
}

public sealed class HomeFeaturedEntryViewModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Tagline { get; init; }
}

public sealed class HomeCarouselViewModel
{
    public required string Image { get; init; }

    public required string Link { get; init; }

    public required string Note { get; init; }
}

public sealed class HomeAnnouncementViewModel
{
    public required string Name { get; init; }

    public required string Url { get; init; }

    public required string BriefIntroduction { get; init; }
}

public sealed class HomeArticleViewModel
{
    public required string Name { get; init; }

    public required string Url { get; init; }

    public required string Author { get; init; }

    public required string PublishTime { get; init; }
}
