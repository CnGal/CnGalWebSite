namespace CnGalWebSite.SDK.MainSite.Models;

using CnGalWebSite.DataModel.ViewModel.Ranks;

/// <summary>
/// 分享游戏库页面 — Steam 游戏总览 ViewModel。
/// </summary>
public sealed class SteamGamesOverviewViewModel
{
    /// <summary>已收录游戏总数。</summary>
    public int TotalGameCount { get; init; }

    /// <summary>库存最多的用户排行。</summary>
    public IReadOnlyList<TopUserItem> TopUsers { get; init; } = [];

    /// <summary>拥有率最高的游戏排行。</summary>
    public IReadOnlyList<TopGameItem> TopGames { get; init; } = [];
}

/// <summary>库存排行中的用户条目。</summary>
public sealed class TopUserItem
{
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserImage { get; init; } = string.Empty;
    public string PersonalSignature { get; init; } = string.Empty;
    public int GameCount { get; init; }
    public IReadOnlyList<RankViewModel> Ranks { get; init; } = [];
}

/// <summary>拥有率排行中的游戏条目。</summary>
public sealed class TopGameItem
{
    public int GameId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public string GameImage { get; init; } = string.Empty;
    public double PossessionRate { get; init; }
}

/// <summary>随机评价条目。</summary>
public sealed class RandomReviewItem
{
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string UserImage { get; init; } = string.Empty;
    public int GameId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public double TotalScore { get; init; }
    public double MusicScore { get; init; }
    public double PaintScore { get; init; }
    public double ScriptScore { get; init; }
    public double ShowScore { get; init; }
    public double SystemScore { get; init; }
    public double CVScore { get; init; }
    public bool IsDubbing { get; init; }
    public string PlayImpressions { get; init; } = string.Empty;
    public DateTime LastEditTime { get; init; }
    public IReadOnlyList<RankViewModel> Ranks { get; init; } = [];
}

