using CnGalWebSite.DataModel.ViewModel.Tags;

namespace CnGalWebSite.SDK.MainSite.Models.TagEdit;

public sealed class TagEditMetaOptions
{
    public List<string> TagItems { get; set; } = [];

    public List<string> EntryItems { get; set; } = [];

    public List<TagTreeModel> TagTreeItems { get; set; } = [];
}
