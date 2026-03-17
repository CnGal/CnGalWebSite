using CnGalWebSite.DataModel.ViewModel.Tags;

namespace CnGalWebSite.SDK.MainSite.Models.TagEdit;

public sealed class TagEditViewModel
{
    public bool IsCreate { get; set; }

    public CreateTagViewModel Data { get; set; } = new();

    public TagEditMetaOptions MetaOptions { get; set; } = new();
}
