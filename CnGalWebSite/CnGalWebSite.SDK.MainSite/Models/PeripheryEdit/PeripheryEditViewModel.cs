using CnGalWebSite.DataModel.ViewModel.Peripheries;

namespace CnGalWebSite.SDK.MainSite.Models.PeripheryEdit;

public sealed class PeripheryEditViewModel
{
    public bool IsCreate { get; set; }

    public CreatePeripheryViewModel Data { get; set; } = new();

    public PeripheryEditMetaOptions MetaOptions { get; set; } = new();
}
