using CnGalWebSite.DataModel.ViewModel.Peripheries;

namespace CnGalWebSite.SDK.MainSite.Models.PeripheryEdit;

public sealed class PeripheryEditRequest
{
    public bool IsCreate { get; set; }

    public CreatePeripheryViewModel Data { get; set; } = new();
}
