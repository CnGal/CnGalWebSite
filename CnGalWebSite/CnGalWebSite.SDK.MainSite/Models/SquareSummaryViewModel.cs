using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class SquareSummaryViewModel
{
    public required IReadOnlyList<SquareRandomTagModel> RandomTags { get; init; }

    public required IReadOnlyList<LotteryCardItemModel> Lotteries { get; init; }

    public required IReadOnlyList<VoteCardItemModel> Votes { get; init; }

    public required IReadOnlyList<SdkErrorModel> Warnings { get; init; }

    /// <summary>
    /// 编辑概览折线图数据（可选，加载失败时为 null）。
    /// </summary>
    public SquareEditOverviewModel? EditOverview { get; init; }
}

public sealed class SquareRandomTagModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required IReadOnlyList<SquareRandomEntryModel> Entries { get; init; }
}

public sealed class SquareRandomEntryModel
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string MainImage { get; init; }

    public required EntryType Type { get; init; }
}

/// <summary>
/// 编辑概览折线图预处理模型：Labels + Series 已转换为 CgLineChart 直接可用的格式。
/// </summary>
public sealed class SquareEditOverviewModel
{
    public required IReadOnlyList<string> Labels { get; init; }

    public required IReadOnlyList<SquareEditOverviewSeriesModel> Series { get; init; }
}

public sealed class SquareEditOverviewSeriesModel
{
    public required string Name { get; init; }

    public required IReadOnlyList<double> Data { get; init; }
}
