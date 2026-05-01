namespace CnGalWebSite.SDK.MainSite.Models.ArticleEdit;

public sealed class ArticleEditMetaOptions
{
    public IReadOnlyList<string> EntryGameItems { get; set; } = [];
    public IReadOnlyList<string> EntryRoleItems { get; set; } = [];
    public IReadOnlyList<string> EntryGroupItems { get; set; } = [];
    public IReadOnlyList<string> EntryStaffItems { get; set; } = [];
    public IReadOnlyList<string> ArticleItems { get; set; } = [];
    public IReadOnlyList<string> VideoItems { get; set; } = [];
}
