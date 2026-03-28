namespace CnGalWebSite.SDK.MainSite.Models.Comments;

/// <summary>
/// 单条评论 ViewModel（含嵌套子评论）。
/// </summary>
public sealed class CommentItemViewModel
{
    public long Id { get; init; }

    public string Text { get; init; } = "";

    public DateTime CommentTime { get; init; }

    public CommentUserViewModel User { get; init; } = new();

    public IReadOnlyList<CommentItemViewModel> Children { get; init; } = [];
}
