using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

/// <summary>
/// 从文本批量解析 Staff 信息，移植自 ToolHelper.GetStaffsFromString。
/// </summary>
public static class StaffBatchParser
{
    /// <summary>
    /// 批量导入 Staff 信息
    /// </summary>
    public static List<StaffModel> GetStaffsFromString(string text)
    {
        var result = new List<StaffModel>();

        text = text.Replace("，", ",").Replace("、", ",").Replace("：", ":").Replace("（", "(").Replace("）", ")").Replace("\r\n", "\n");
        var subcategory = string.Empty;
        var lines = text.Split('\n');

        foreach (var item in lines)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }

            var pairs = item.Split(":");
            string position;
            string? positionGeneral;
            if (pairs.Length == 0)
            {
                continue;
            }
            else if (pairs.Length == 1 || (pairs.Length == 2 && string.IsNullOrWhiteSpace(pairs[1])))
            {
                subcategory = pairs[0];
                continue;
            }
            else if (pairs.Length == 2)
            {
                position = pairs[0];
                positionGeneral = MidStrEx(position, "(", ")");
                if (string.IsNullOrWhiteSpace(positionGeneral))
                {
                    positionGeneral = null;
                }
                else
                {
                    position = position.Replace($"({positionGeneral})", "");
                }
            }
            else
            {
                continue;
            }

            var names = pairs[1].Split(",");
            foreach (var infor in names)
            {
                if (string.IsNullOrWhiteSpace(infor))
                {
                    continue;
                }
                var roleName = MidStrEx(infor, "(", ")");
                var type = StaffHelper.GetGeneralType(positionGeneral ?? position);
                result.Add(new StaffModel
                {
                    Name = infor.Replace($"({roleName})", "").Trim(),
                    PositionOfficial = position.Trim(),
                    Modifier = subcategory.Trim(),
                    SubordinateOrganization = roleName,
                    PositionGeneral = type
                });
            }
        }

        return result;
    }

    private static string MidStrEx(string source, string startStr, string endStr)
    {
        var result = string.Empty;
        try
        {
            var startIndex = source.IndexOf(startStr);
            if (startIndex == -1)
            {
                return result;
            }

            var tmpStr = source[(startIndex + startStr.Length)..];
            var endIndex = tmpStr.IndexOf(endStr);
            if (endIndex == -1)
            {
                return result;
            }

            result = tmpStr.Remove(endIndex);
        }
        catch
        {
            // Ignore parse errors
        }
        return result;
    }
}
