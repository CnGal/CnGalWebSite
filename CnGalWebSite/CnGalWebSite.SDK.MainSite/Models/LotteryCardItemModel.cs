using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class LotteryCardItemModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required string Thumbnail { get; init; }

    public required DateTime BeginTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required long Count { get; init; }

    public required LotteryConditionType ConditionType { get; init; }

    public required bool IsEnd { get; init; }

    public required string GameSteamId { get; init; }
}
