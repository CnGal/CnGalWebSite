using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class UserEditRecordsViewModel
{
    public required int TotalCount { get; init; }
    public required int CurrentPage { get; init; }
    public required int MaxCount { get; init; }
    public int TotalPages => MaxCount <= 0 ? 0 : (int)Math.Ceiling((decimal)TotalCount / MaxCount);
    public required IReadOnlyList<ExaminedNormalListModel> Items { get; init; }
}
