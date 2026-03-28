using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 收藏夹详情页面向前端的 ViewModel。
/// </summary>
public sealed class FavoriteFolderDetailViewModel
{
    public required long Id { get; init; }

    public required string Name { get; init; }

    public required string BriefIntroduction { get; init; }

    public required string MainPicture { get; init; }

    public required DateTime CreateTime { get; init; }

    public required DateTime LastEditTime { get; init; }

    public required int ReaderCount { get; init; }

    public required bool IsHidden { get; init; }

    public required bool Authority { get; init; }

    /// <summary>
    /// 创建者信息
    /// </summary>
    public required string UserName { get; init; }

    public required string UserPhotoPath { get; init; }

    public required string UserId { get; init; }

    /// <summary>
    /// 按类型分组的收藏对象 — 词条
    /// </summary>
    public required IReadOnlyList<EntryInforTipViewModel> Entries { get; init; }

    /// <summary>
    /// 按类型分组的收藏对象 — 文章
    /// </summary>
    public required IReadOnlyList<ArticleInforTipViewModel> Articles { get; init; }

    /// <summary>
    /// 按类型分组的收藏对象 — 视频
    /// </summary>
    public required IReadOnlyList<VideoInforTipViewModel> Videos { get; init; }

    /// <summary>
    /// 按类型分组的收藏对象 — 标签
    /// </summary>
    public required IReadOnlyList<TagInforTipViewModel> Tags { get; init; }

    /// <summary>
    /// 按类型分组的收藏对象 — 周边
    /// </summary>
    public required IReadOnlyList<PeripheryInforTipViewModel> Peripheries { get; init; }

    /// <summary>
    /// 总收藏对象数
    /// </summary>
    public int TotalObjectCount => Entries.Count + Articles.Count + Videos.Count + Tags.Count + Peripheries.Count;
}
