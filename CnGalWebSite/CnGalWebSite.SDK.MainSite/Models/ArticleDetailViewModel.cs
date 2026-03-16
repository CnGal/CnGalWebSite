using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class ArticleDetailViewModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required string BackgroundPicture { get; init; }

    public required string SmallBackgroundPicture { get; init; }

    public required string MainPage { get; init; }

    public required ArticleType Type { get; init; }

    public required DateTime CreateTime { get; init; }

    public required DateTime LastEditTime { get; init; }

    public required string OriginalAuthor { get; init; }

    public required string OriginalLink { get; init; }

    public required DateTime? PubishTime { get; init; }

    public required int ReaderCount { get; init; }

    public required int ThumbsUpCount { get; init; }

    public required long CommentCount { get; init; }

    public required bool IsHidden { get; init; }

    public required bool CanComment { get; init; }

    /// <summary>
    /// 作者用户名
    /// </summary>
    public required string AuthorName { get; init; }

    /// <summary>
    /// 作者头像 URL
    /// </summary>
    public required string AuthorAvatar { get; init; }

    /// <summary>
    /// 作者 Space 用户 ID（用于链接到个人空间）
    /// </summary>
    public required string AuthorId { get; init; }

    public required IReadOnlyList<EntryInforTipViewModel> RelatedEntries { get; init; }

    public required IReadOnlyList<ArticleInforTipViewModel> RelatedArticles { get; init; }

    public required IReadOnlyList<VideoInforTipViewModel> RelatedVideos { get; init; }

    public required IReadOnlyList<RelevancesKeyValueModel> RelatedOutlinks { get; init; }
}
