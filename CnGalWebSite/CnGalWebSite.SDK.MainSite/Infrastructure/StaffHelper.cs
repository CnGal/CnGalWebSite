using CnGalWebSite.DataModel.Model;
using CnGalWebSite.Extensions;
using System;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

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
        else if ((text.Contains("配音") || text.Contains("声优") || text.ToUpper().Contains("CV") || text.ToUpper().Contains("CAST")) 
            && !text.Contains("导演") && !text.Contains("监督") && !text.Contains("制作") && !text.Contains("后期") && !text.Contains("处理") && !text.Contains("后制"))
        {
            return PositionGeneralType.CV;
        }
        else if (text.Contains("感谢") || text.ToUpper().Contains("鸣谢") || text.ToUpper().Contains("致谢"))
        {
            return PositionGeneralType.SpecialThanks;
        }

        return PositionGeneralType.Other;
    }
}
