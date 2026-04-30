using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.SDK.MainSite.Models.Toolbox;

public sealed class ToolboxBilibiliVideoInfo
{
    public string BilibiliId { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public CopyrightType Copyright { get; init; }
    public string MainPicture { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTime PublishTime { get; init; }
    public string Description { get; init; } = string.Empty;
    public TimeSpan Duration { get; init; }
    public bool IsInteractive { get; init; }
    public string OriginalAuthor { get; init; } = string.Empty;
}
