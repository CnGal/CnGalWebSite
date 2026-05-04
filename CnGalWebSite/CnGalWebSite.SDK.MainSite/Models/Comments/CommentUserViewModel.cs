using CnGalWebSite.DataModel.ViewModel.Ranks;

namespace CnGalWebSite.SDK.MainSite.Models.Comments;

/// <summary>
/// 评论用户信息 ViewModel。
/// </summary>
public sealed class CommentUserViewModel
{
    public string Id { get; init; } = "";

    public string Name { get; init; } = "";

    public string PhotoPath { get; init; } = "";

    public IReadOnlyList<RankViewModel> Ranks { get; init; } = [];
}
