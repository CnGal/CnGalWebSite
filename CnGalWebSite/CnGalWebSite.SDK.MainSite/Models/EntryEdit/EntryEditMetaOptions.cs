using CnGalWebSite.DataModel.ViewModel.Tags;

namespace CnGalWebSite.SDK.MainSite.Models.EntryEdit;

public sealed class EntryEditMetaOptions
{
    public IReadOnlyList<string> EntryGameItems { get; set; } = [];
    public IReadOnlyList<string> EntryRoleItems { get; set; } = [];
    public IReadOnlyList<string> EntryGroupItems { get; set; } = [];
    public IReadOnlyList<string> EntryStaffItems { get; set; } = [];
    public IReadOnlyList<string> ArticleItems { get; set; } = [];
    public IReadOnlyList<string> TagItems { get; set; } = [];
    public IReadOnlyList<string> VideoItems { get; set; } = [];
    public IReadOnlyList<string> LotteryItems { get; set; } = [];
    public IReadOnlyList<TagTreeModel> TagTreeItems { get; set; } = [];
}
