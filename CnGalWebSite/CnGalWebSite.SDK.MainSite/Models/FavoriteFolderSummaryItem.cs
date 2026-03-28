using System;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 收藏夹概览项（个人空间收藏夹列表中的每一项）。
/// </summary>
public sealed class FavoriteFolderSummaryItem
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BriefIntroduction { get; set; } = string.Empty;

    public string MainImage { get; set; } = string.Empty;

    public long Count { get; set; }

    public DateTime CreateTime { get; set; }

    public bool IsDefault { get; set; }

    public bool ShowPublicly { get; set; }

    public bool IsHidden { get; set; }
}
