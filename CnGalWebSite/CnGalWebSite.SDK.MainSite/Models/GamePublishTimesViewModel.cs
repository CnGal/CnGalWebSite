namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 游戏发布时间卡片列表项（CardList 模式）
/// </summary>
public sealed class GamePublishTimesCardItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Image { get; set; } = "";
    public string BriefIntroduction { get; set; } = "";
    public DateTime? PublishTime { get; set; }
}

/// <summary>
/// 游戏发布时间时间线项目（Timeline 模式）
/// </summary>
public sealed class GamePublishTimesTimelineItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Image { get; set; } = "";
    public string BriefIntroduction { get; set; } = "";
    public string Thumbnail { get; set; } = "";
    public DateTime? PublishTime { get; set; }
    public string? PublishTimeNote { get; set; }
}
