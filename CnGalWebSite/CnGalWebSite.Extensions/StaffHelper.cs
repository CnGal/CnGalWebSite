using CnGalWebSite.DataModel.Model;
using System;

namespace CnGalWebSite.Extensions;

/// <summary>
/// 从职位展示文本推断通用职位类型（与词条详情 Staff 分组逻辑一致）。
/// </summary>
public static class StaffHelper
{
    public static PositionGeneralType GetGeneralType(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return PositionGeneralType.Other;
        }

        foreach (var item in Enum.GetValues(typeof(PositionGeneralType)))
        {
            var temp = (PositionGeneralType)item;
            if (text == temp.GetDisplayName())
            {
                return temp;
            }
        }

        if (text.Contains("原画") || text.Contains("画师") || text.Contains("设计"))
        {
            return PositionGeneralType.FineArts;
        }
        else if ((text.Contains("配音") || text.Contains("声优") || text.ToUpperInvariant().Contains("CV") || text.ToUpperInvariant().Contains("CAST"))
            && !text.Contains("导演") && !text.Contains("监督") && !text.Contains("制作") && !text.Contains("后期") && !text.Contains("处理") && !text.Contains("后制"))
        {
            return PositionGeneralType.CV;
        }
        else if (text.Contains("感谢") || text.ToUpperInvariant().Contains("鸣谢") || text.ToUpperInvariant().Contains("致谢"))
        {
            return PositionGeneralType.SpecialThanks;
        }

        return PositionGeneralType.Other;
    }
}
