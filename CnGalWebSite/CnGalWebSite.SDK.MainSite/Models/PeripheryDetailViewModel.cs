using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class PeripheryDetailViewModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string DisplayName { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required string BackgroundPicture { get; init; }

    public required string SmallBackgroundPicture { get; init; }

    public required bool IsHidden { get; init; }

    // ── 类型与属性 ──

    public required PeripheryType Type { get; init; }

    public required string Category { get; init; }

    public required string Brand { get; init; }

    public required string Author { get; init; }

    public required string Material { get; init; }

    public required string Size { get; init; }

    public required string IndividualParts { get; init; }

    public required string Price { get; init; }

    public required string SaleLink { get; init; }

    public required bool IsReprint { get; init; }

    public required bool IsAvailableItem { get; init; }

    /// <summary>
    /// 页数（设定集/画册）
    /// </summary>
    public required int PageCount { get; init; }

    /// <summary>
    /// 歌曲数（原声集）
    /// </summary>
    public required int SongCount { get; init; }

    // ── 统计 ──

    public required int ReaderCount { get; init; }

    public required int CollectedCount { get; init; }

    public required int CommentCount { get; init; }

    public required bool CanComment { get; init; }

    // ── 关联数据 ──

    public required IReadOnlyList<EntryInforTipViewModel> Entries { get; init; }

    public required IReadOnlyList<PeripheryInforTipViewModel> Peripheries { get; init; }

    /// <summary>
    /// 按游戏/用户分组的周边收集概览
    /// </summary>
    public required IReadOnlyList<GameOverviewPeripheryListModel> PeripheryOverviewModels { get; init; }

    // ── 图片 ──

    public required IReadOnlyList<PicturesViewModel> Pictures { get; init; }
}
