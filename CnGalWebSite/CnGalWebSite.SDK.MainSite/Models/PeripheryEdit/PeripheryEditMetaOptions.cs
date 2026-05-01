namespace CnGalWebSite.SDK.MainSite.Models.PeripheryEdit;

public sealed class PeripheryEditMetaOptions
{
    public IReadOnlyList<string> EntryGameItems { get; set; } = [];
    public IReadOnlyList<string> EntryRoleItems { get; set; } = [];
    public IReadOnlyList<string> EntryGroupItems { get; set; } = [];
    public IReadOnlyList<string> EntryStaffItems { get; set; } = [];
    public IReadOnlyList<string> PeripheryItems { get; set; } = [];
}
