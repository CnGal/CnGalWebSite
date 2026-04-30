using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.Extensions;

/// <summary>
/// 词条详情页「配音作品」分组与 CV 专题页作品数的统一筛选逻辑。
/// </summary>
public static class StaffCastWorksHelper
{
    public static bool IsCastWork(EntryInforTipViewModel staffGame) =>
        staffGame.AddInfors != null
        && staffGame.AddInfors.Any(a =>
            a.Contents.Any(c => StaffHelper.GetGeneralType(c.DisplayName) == PositionGeneralType.CV));

    public static bool IsCastWorkFromStaffTuples(
        IEnumerable<(string? Modifier, string? PositionOfficial, PositionGeneralType PositionGeneral)> rows) =>
        rows.Any(r =>
        {
            var d = (string.IsNullOrWhiteSpace(r.Modifier) ? "" : r.Modifier + " - ")
                + (r.PositionOfficial ?? r.PositionGeneral.GetDisplayName());
            return StaffHelper.GetGeneralType(d) == PositionGeneralType.CV;
        });
}
