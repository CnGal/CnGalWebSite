using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class UserArticlesViewModel
{
    public required int TotalCount { get; init; }
    public required int CurrentPage { get; init; }
    public required int MaxCount { get; init; }
    public int TotalPages => MaxCount <= 0 ? 0 : (int)Math.Ceiling((decimal)TotalCount / MaxCount);
    public required IReadOnlyList<ArticleInforTipViewModel> Items { get; init; }
}
