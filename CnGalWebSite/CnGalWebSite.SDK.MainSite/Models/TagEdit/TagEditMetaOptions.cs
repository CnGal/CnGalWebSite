using CnGalWebSite.DataModel.ViewModel.Tags;

namespace CnGalWebSite.SDK.MainSite.Models.TagEdit;

public sealed class TagEditMetaOptions
{
    public IReadOnlyList<string> TagItems { get; set; } = [];

    public IReadOnlyList<string> EntryItems { get; set; } = [];

    public IReadOnlyList<TagTreeModel> TagTreeItems { get; set; } = [];
}
