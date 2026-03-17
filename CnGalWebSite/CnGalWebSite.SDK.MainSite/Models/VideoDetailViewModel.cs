using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class VideoDetailViewModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required string BackgroundPicture { get; init; }

    public required string SmallBackgroundPicture { get; init; }

    public required string MainPage { get; init; }

    /// <summary>
    /// 视频类型（如"攻略""鉴赏"等自定义文本）
    /// </summary>
    public required string Type { get; init; }

    public required CopyrightType Copyright { get; init; }

    public required TimeSpan Duration { get; init; }

    public required bool IsInteractive { get; init; }

    public required bool IsCreatedByCurrentUser { get; init; }

    public required DateTime CreateTime { get; init; }

    public required DateTime LastEditTime { get; init; }

    public required DateTime PubishTime { get; init; }

    public required int ReaderCount { get; init; }

    public required int CommentCount { get; init; }

    public required string OriginalAuthor { get; init; }

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

    /// <summary>
    /// 是否为原创作者（true = "作者"标签，false = "搬运"标签）
    /// </summary>
    public required bool IsAuthorOriginal { get; init; }

    public required IReadOnlyList<EntryInforTipViewModel> RelatedEntries { get; init; }

    public required IReadOnlyList<ArticleInforTipViewModel> RelatedArticles { get; init; }

    public required IReadOnlyList<VideoInforTipViewModel> RelatedVideos { get; init; }

    public required IReadOnlyList<RelevancesKeyValueModel> RelatedOutlinks { get; init; }

    public required IReadOnlyList<PicturesAloneViewModel> Pictures { get; init; }
}
