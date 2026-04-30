using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.MainSite.Shared.Models.Toolbox;

public enum ToolboxOutputLevel
{
    Info,
    Warning,
    Error
}

public enum ToolboxDialogKind
{
    None,
    MergeEntry,
    RepostArticle,
    RepostVideo
}

public abstract class ToolboxTaskBase
{
    public int TotalTaskCount { get; set; }
    public int CompleteTaskCount { get; set; }
    public string Error { get; set; } = string.Empty;
    public DateTime? PostTime { get; set; }

    public double Progress => TotalTaskCount <= 0 ? 0 : Math.Clamp(CompleteTaskCount * 100.0 / TotalTaskCount, 0, 100);
    public bool Succeeded => PostTime.HasValue && string.IsNullOrWhiteSpace(Error);
    public bool Failed => !string.IsNullOrWhiteSpace(Error);
}

public sealed class RepostArticleTaskModel : ToolboxTaskBase
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalAuthor { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string MainPage { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool IsCutImage { get; set; }
    public DateTime PublishTime { get; set; }
    public ArticleType Type { get; set; } = ArticleType.Evaluation;
    public string UserName { get; set; } = string.Empty;
    public bool IsCreatedByCurrentUser { get; set; }

    public RepostArticleTaskModel()
    {
        TotalTaskCount = 4;
    }
}

public sealed class RepostVideoTaskModel : ToolboxTaskBase
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsCreatedByCurrentUser { get; set; }

    public RepostVideoTaskModel()
    {
        TotalTaskCount = 5;
    }
}

public sealed class MergeEntryTaskModel : ToolboxTaskBase
{
    public int HostId { get; set; }
    public string HostName { get; set; } = string.Empty;
    public int SubId { get; set; }
    public string SubName { get; set; } = string.Empty;

    public MergeEntryTaskModel()
    {
        TotalTaskCount = 10;
    }
}

public sealed class OriginalImageMapModel
{
    public string OldUrl { get; set; } = string.Empty;
    public string NewUrl { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
}

public sealed record ToolboxProgressUpdate(ToolboxOutputLevel Level, string Message);
