namespace CnGalWebSite.SDK.MainSite.Models.VideoEdit;

public sealed class VideoEditMetaOptions
{
    public IReadOnlyList<string> EntryGameItems { get; set; } = [];
    public IReadOnlyList<string> EntryRoleItems { get; set; } = [];
    public IReadOnlyList<string> EntryGroupItems { get; set; } = [];
    public IReadOnlyList<string> EntryStaffItems { get; set; } = [];
    public IReadOnlyList<string> ArticleItems { get; set; } = [];
    public IReadOnlyList<string> VideoItems { get; set; } = [];
}
