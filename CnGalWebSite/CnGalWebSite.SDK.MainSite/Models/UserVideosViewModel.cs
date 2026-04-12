using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class UserVideosViewModel
{
    public required IReadOnlyList<VideoInforTipViewModel> Items { get; init; }
}
