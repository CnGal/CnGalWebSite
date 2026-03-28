using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.SDK.MainSite.Models;

public sealed class SpaceDetailViewModel
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string PersonalSignature { get; init; }

    public required string BackgroundImage { get; init; }

    public required string MBgImage { get; init; }

    public required string SBgImage { get; init; }

    public required string PhotoPath { get; init; }

    public required string MainPageHtml { get; init; }

    public required int Integral { get; init; }

    public required int GCoins { get; init; }

    public required int ContributionValue { get; init; }

    public required int EditCount { get; init; }

    public required int ArticleCount { get; init; }

    public required int VideoCount { get; init; }

    public required int SignInDays { get; init; }

    public required DateTime? Birthday { get; init; }

    public required DateTime RegisteTime { get; init; }

    public required DateTime LastOnlineTime { get; init; }

    public required double OnlineTime { get; init; }

    public required IReadOnlyList<RankViewModel> Ranks { get; init; }

    public required EntryInforTipViewModel? UserCertification { get; init; }

    public required bool IsCurrentUser { get; init; }

    public required string SteamId { get; init; }

    public required bool CanComment { get; init; }
}
