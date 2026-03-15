using CnGalWebSite.DataModel.ViewModel;

namespace CnGalWebSite.SDK.MainSite.Models.EntryEdit;

public sealed class EntryEditRequest
{
    public bool IsCreate { get; set; }
    
    public EstablishEntryViewModel Data { get; set; } = new();
}
