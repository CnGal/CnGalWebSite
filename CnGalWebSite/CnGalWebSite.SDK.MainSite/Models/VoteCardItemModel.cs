namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class VoteCardItemModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required DateTime BeginTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required long Count { get; init; }

    public required bool IsEnd { get; init; }
}
