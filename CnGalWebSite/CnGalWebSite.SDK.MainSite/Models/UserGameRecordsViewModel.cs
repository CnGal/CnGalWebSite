using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 用户游玩记录列表 ViewModel（面向 SpaceIndexPage 静态展示）。
/// </summary>
public sealed class UserGameRecordsViewModel
{
    public required IReadOnlyList<GameRecordItem> Records { get; init; }
}

/// <summary>
/// 单条游戏记录。
/// </summary>
public sealed class GameRecordItem
{
    public required int GameId { get; init; }
    public required string GameName { get; init; }
    public required string GameImage { get; init; }
    public required string GameBriefIntroduction { get; init; }
    public required PlayedGameType Type { get; init; }

    /// <summary>
    /// Steam 游玩时长（分钟）。
    /// </summary>
    public required long PlayDuration { get; init; }

    /// <summary>
    /// 是否在 Steam 库中。
    /// </summary>
    public required bool IsInSteam { get; init; }

    /// <summary>
    /// 是否有配音。
    /// </summary>
    public required bool IsDubbing { get; init; }

    // 评分
    public required int MusicScore { get; init; }
    public required int PaintScore { get; init; }
    public required int ScriptScore { get; init; }
    public required int ShowScore { get; init; }
    public required int SystemScore { get; init; }
    public required int CVScore { get; init; }
    public required int TotalScore { get; init; }

    public required string? PlayImpressions { get; init; }
    public required bool ShowPublicly { get; init; }
    public required bool IsHidden { get; init; }

    public bool IsScored => MusicScore != 0 && ShowScore != 0 && TotalScore != 0
                         && PaintScore != 0 && ScriptScore != 0 && CVScore != 0 && SystemScore != 0;
}

/// <summary>
/// Steam 用户信息（面向 SpaceIndexPage 静态展示）。
/// </summary>
public sealed class SteamUserInfoItem
{
    public required string SteamId { get; init; }
    public required string Name { get; init; }
    public required string Image { get; init; }
    public double Price { get; init; }
}
