namespace CnGalWebSite.SDK.MainSite.Models.Comments;

/// <summary>
/// 评论列表 ViewModel，由 SDK QueryService 返回。
/// </summary>
public sealed class CommentListViewModel
{
    public IReadOnlyList<CommentItemViewModel> Items { get; init; } = [];

    public int TotalCount { get; init; }
}
