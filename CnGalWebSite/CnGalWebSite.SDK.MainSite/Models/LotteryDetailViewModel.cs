using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class LotteryDetailViewModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required string BackgroundPicture { get; init; }

    public required string SmallBackgroundPicture { get; init; }

    public required string MainPage { get; init; }

    public required LotteryType Type { get; init; }

    public required LotteryConditionType ConditionType { get; init; }

    public required DateTime BeginTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required DateTime LotteryTime { get; init; }

    public required DateTime CreateTime { get; init; }

    public required DateTime LastEditTime { get; init; }

    public required bool IsEnd { get; init; }

    public required bool IsHidden { get; init; }

    public required bool CanComment { get; init; }

    public required long Count { get; init; }

    public required int ReaderCount { get; init; }

    public required int CommentCount { get; init; }

    public required IReadOnlyList<LotteryAwardDetailModel> Awards { get; init; }
}

public sealed class LotteryAwardDetailModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required int Priority { get; init; }

    public required int Count { get; init; }

    public required LotteryAwardType Type { get; init; }

    public required string Sponsor { get; init; }

    public required string Image { get; init; }

    public required string Link { get; init; }

    public required int Integral { get; init; }

    public required IReadOnlyList<LotteryWinnerModel> Winners { get; init; }
}

public sealed class LotteryWinnerModel
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string PhotoPath { get; init; }

    public IReadOnlyList<RankViewModel> Ranks { get; init; } = [];
}
