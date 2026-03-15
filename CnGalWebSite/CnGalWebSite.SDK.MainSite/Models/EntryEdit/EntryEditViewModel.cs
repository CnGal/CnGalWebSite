using CnGalWebSite.DataModel.ViewModel;

namespace CnGalWebSite.SDK.MainSite.Models.EntryEdit;

public sealed class EntryEditViewModel
{
    public bool IsCreate { get; set; }
    
    public EstablishEntryViewModel Data { get; set; } = new();

    public EntryEditMetaOptions MetaOptions { get; set; } = new();
}
