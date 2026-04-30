using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.Extensions;

/// <summary>
/// 词条详情页「参与作品」分组筛选逻辑。
/// </summary>
public static class StaffParticipationWorksHelper
{
    /// <summary>
    /// 与 <c>EntryDetailPage.GetParticipationWorks</c> 中单条 StaffGame 的判定一致。
    /// </summary>
    public static bool IsParticipationWork(EntryInforTipViewModel staffGame)
    {
        if (staffGame.AddInfors == null)
        {
            return true;
        }

        return staffGame.AddInfors.Any(a =>
            a.Contents.Any(c =>
            {
                var t = StaffHelper.GetGeneralType(c.DisplayName);
                return t != PositionGeneralType.CV && t != PositionGeneralType.SpecialThanks;
            })
            && a.Contents.Any(c =>
            {
                var t = StaffHelper.GetGeneralType(c.DisplayName);
                return t == PositionGeneralType.ProductionGroup || t == PositionGeneralType.Publisher;
            }) == false);
    }

    /// <summary>
    /// 根据游戏侧 Staff 投影（与详情页构建 DisplayName 的规则一致）判断是否计入参与作品。
    /// </summary>
    public static bool IsParticipationWorkFromStaffTuples(
        IEnumerable<(string? Modifier, string? PositionOfficial, PositionGeneralType PositionGeneral)> rows)
    {
        var displayNames = rows.Select(r =>
            (string.IsNullOrWhiteSpace(r.Modifier) ? "" : r.Modifier + " - ")
            + (r.PositionOfficial ?? r.PositionGeneral.GetDisplayName())).ToList();

        if (displayNames.Count == 0)
        {
            return false;
        }

        return displayNames.Any(d =>
        {
            var t = StaffHelper.GetGeneralType(d);
            return t != PositionGeneralType.CV && t != PositionGeneralType.SpecialThanks;
        })
        && displayNames.Any(d =>
        {
            var t = StaffHelper.GetGeneralType(d);
            return t == PositionGeneralType.ProductionGroup || t == PositionGeneralType.Publisher;
        }) == false;
    }
}
