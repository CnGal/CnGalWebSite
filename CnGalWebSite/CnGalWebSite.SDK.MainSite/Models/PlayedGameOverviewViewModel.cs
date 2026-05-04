namespace CnGalWebSite.SDK.MainSite.Models;

using CnGalWebSite.DataModel.ViewModel.Ranks;

/// <summary>
/// 词条评分概览 ViewModel（面向 EntryDetailPage 静态展示）。
/// </summary>
public sealed class PlayedGameOverviewViewModel
{
    public required string GameName { get; init; }
    public required int GameId { get; init; }

    /// <summary>
    /// 全部评分的聚合分。
    /// </summary>
    public required ScoreSummary TotalScores { get; init; }

    /// <summary>
    /// 过滤后（仅含有效评语）的聚合分。
    /// </summary>
    public required ScoreSummary FilteredScores { get; init; }

    /// <summary>
    /// 是否配音（仅游戏词条）。
    /// </summary>
    public required bool IsDubbing { get; init; }

    /// <summary>
    /// 用户评分列表。
    /// </summary>
    public required IReadOnlyList<UserScoreItem> UserScores { get; init; }

    /// <summary>
    /// 当前登录用户是否已有评分。
    /// </summary>
    public required bool IsCurrentUserScoreExist { get; init; }

    /// <summary>
    /// 当前登录用户的评分是否公开。
    /// </summary>
    public required bool IsCurrentUserScorePublic { get; init; }

    /// <summary>
    /// 当前登录用户 Id。
    /// </summary>
    public required string? CurrentUserId { get; init; }

    /// <summary>
    /// 评分人数。
    /// </summary>
    public int ScoredCount => UserScores.Count(s => s.IsScored);
}

/// <summary>
/// 评分维度汇总（用于聚合展示）。
/// </summary>
public sealed class ScoreSummary
{
    public required double MusicScore { get; init; }
    public required double PaintScore { get; init; }
    public required double ScriptScore { get; init; }
    public required double ShowScore { get; init; }
    public required double SystemScore { get; init; }
    public required double CVScore { get; init; }
    public required double TotalScore { get; init; }
    public bool IsScored => MusicScore != 0 && ShowScore != 0 && TotalScore != 0
                         && PaintScore != 0 && ScriptScore != 0 && CVScore != 0 && SystemScore != 0;
}

/// <summary>
/// 单个用户的评分条目。
/// </summary>
public sealed class UserScoreItem
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string UserPhoto { get; init; }

    public IReadOnlyList<RankViewModel> Ranks { get; init; } = [];

    public required double MusicScore { get; init; }
    public required double PaintScore { get; init; }
    public required double ScriptScore { get; init; }
    public required double ShowScore { get; init; }
    public required double SystemScore { get; init; }
    public required double CVScore { get; init; }
    public required double TotalScore { get; init; }

    public required string? PlayImpressions { get; init; }
    public required DateTime LastEditTime { get; init; }

    public bool IsScored => MusicScore != 0 && ShowScore != 0 && TotalScore != 0
                         && PaintScore != 0 && ScriptScore != 0 && CVScore != 0 && SystemScore != 0;
}
