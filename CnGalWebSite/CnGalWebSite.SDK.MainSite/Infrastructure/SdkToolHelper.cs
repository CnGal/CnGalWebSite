using System;
using System.Text;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.SDK.MainSite.Infrastructure;

public static class SdkToolHelper
{
    public static string Base64EncodeUrl(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return "A" + Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_");
    }

    public static string Base64DecodeUrl(string content)
    {
        content = content[1..];
        content = content.Replace("-", "+").Replace("_", "/").Replace("%3d", "=").Replace("%3D", "=");
        var bytes = Convert.FromBase64String(content);
        var result = Encoding.UTF8.GetString(bytes);
        
        // Validate Url
        if (string.IsNullOrWhiteSpace(result))
        {
            return result;
        }
        else
        {
            if (result.StartsWith("/"))
            {
                return result;
            }
            else
            {
                if (result.StartsWith("https://app.cngal.org") || result.StartsWith("http://app.cngal.org") || result.StartsWith("https://m.cngal.org") || result.StartsWith("http://m.cngal.org") || result.StartsWith("https://www.cngal.org") || result.StartsWith("http://www.cngal.org") || result.StartsWith("https://localhost:") || result.StartsWith("http://localhost:"))
                {
                    return result;
                }
                else
                {
                    return "/";
                }
            }
        }
    }

    public static PerfectionLevel GetEntryPerfectionLevel(double grade)
    {
        if (grade < 60)
        {
            return PerfectionLevel.ToBeImproved;
        }
        else if (grade < 80)
        {
            return PerfectionLevel.Good;
        }
        else
        {
            return PerfectionLevel.Excellent;
        }
    }

    public static string GetEntryPerfectionLevelColor(PerfectionLevel level)
    {
        return level switch
        {
            PerfectionLevel.ToBeImproved => "#e53935",
            PerfectionLevel.Good => "#1e88e5",
            PerfectionLevel.Excellent => "#43a047",
            _ => "#757575",
        };
    }

    public static PerfectionCheckLevel GetEntryPerfectionCheckLevel(PerfectionCheckType checkType, PerfectionDefectType defectType)
    {
        if (defectType == PerfectionDefectType.None)
        {
            return PerfectionCheckLevel.None;
        }
        else if (checkType == PerfectionCheckType.BriefIntroduction || checkType == PerfectionCheckType.MainImage
            || checkType == PerfectionCheckType.IssueTime || checkType == PerfectionCheckType.MainPage)
        {
            return PerfectionCheckLevel.High;
        }
        else if (checkType == PerfectionCheckType.Staff || checkType == PerfectionCheckType.SteamId
            || checkType == PerfectionCheckType.ProductionGroup)
        {
            return PerfectionCheckLevel.Middle;
        }
        else
        {
            return PerfectionCheckLevel.Low;
        }
    }

    public static string GetEntryPerfectionCheckLevelColor(PerfectionCheckLevel level)
    {
        return level switch
        {
            PerfectionCheckLevel.None => "#43a047",
            PerfectionCheckLevel.High => "#e53935",
            PerfectionCheckLevel.Middle => "#fb8c00",
            PerfectionCheckLevel.Low => "#1e88e5",
            _ => "#757575",
        };
    }

    public static string GetPerfectionCheckTitle(PerfectionCheckViewModel model)
    {
        return model.DefectType switch
        {
            PerfectionDefectType.None => model.CheckType.GetDisplayName() + " 已填写",
            _ => model.CheckType switch
            {
                PerfectionCheckType.RelevanceEntries => string.IsNullOrWhiteSpace(model.Infor) ? (model.CheckType.GetDisplayName() + " " + model.DefectType.GetDisplayName()) : ("关联词条 " + model.Infor + " " + model.DefectType.GetDisplayName()),
                PerfectionCheckType.RelevanceArticles => string.IsNullOrWhiteSpace(model.Infor) ? (model.CheckType.GetDisplayName() + " " + model.DefectType.GetDisplayName()) : ("关联文章 " + model.Infor + " " + model.DefectType.GetDisplayName()),
                PerfectionCheckType.ProductionGroup => string.IsNullOrWhiteSpace(model.Infor) ? (model.CheckType.GetDisplayName() + " " + model.DefectType.GetDisplayName()) : ("制作组 " + model.Infor + " " + model.DefectType.GetDisplayName()),
                PerfectionCheckType.Publisher => string.IsNullOrWhiteSpace(model.Infor) ? (model.CheckType.GetDisplayName() + " " + model.DefectType.GetDisplayName()) : ("发行商 " + model.Infor + " " + model.DefectType.GetDisplayName()),
                PerfectionCheckType.Staff => string.IsNullOrWhiteSpace(model.Infor) ? (model.CheckType.GetDisplayName() + " " + model.DefectType.GetDisplayName()) : ("关联Staff " + model.Infor + " " + model.DefectType.GetDisplayName()),
                _ => model.CheckType.GetDisplayName() + " " + model.DefectType.GetDisplayName(),
            },
        };
    }

    public static string GetPerfectionCheckContext(PerfectionCheckViewModel model)
    {
        return model.DefectType switch
        {
            PerfectionDefectType.InsufficientLength => model.CheckType switch
            {
                PerfectionCheckType.BriefIntroduction => "简介长度建议30字以上，可以多写一些游戏的介绍哦",
                PerfectionCheckType.MainPage => "主页长度建议100字以上，可以介绍一下游戏的故事背景，出场角色等内容哦",
                _ => model.CheckType.GetDisplayName() + "建议多写一些哦",
            },
            PerfectionDefectType.None => "全站 " + model.VictoryPercentage.ToString("0.0") + "% 的词条已填写此项",
            PerfectionDefectType.TypeError => "关联的 " + model.Infor + " 类型错误，请检查是否重名",
            _ => model.CheckType switch
            {
                PerfectionCheckType.RelevanceEntries => string.IsNullOrWhiteSpace(model.Infor) ? ("需要 填写关联词条") : (model.Count > 1 ? ("全站约有" + (model.Count - 1).ToString() + "个词条也关联了 " + model.Infor) : ("全站约有" + model.Count + "个词条关联了 " + model.Infor)),
                PerfectionCheckType.RelevanceArticles => string.IsNullOrWhiteSpace(model.Infor) ? ("需要 填写关联文章") : (model.Count > 1 ? ("全站约有" + (model.Count - 1).ToString() + "个词条也关联了 " + model.Infor) : ("全站约有" + model.Count + "个词条关联了 " + model.Infor)),
                PerfectionCheckType.ProductionGroup => string.IsNullOrWhiteSpace(model.Infor) ? ("需要 填写制作组") : (model.Count > 1 ? ("全站约有" + (model.Count - 1).ToString() + "个词条也关联了 " + model.Infor) : ("全站约有" + model.Count + "个词条关联了 " + model.Infor)),
                PerfectionCheckType.Publisher => string.IsNullOrWhiteSpace(model.Infor) ? ("需要 填写发行商") : (model.Count > 1 ? ("全站约有" + (model.Count - 1).ToString() + "个词条也关联了 " + model.Infor) : ("全站约有" + model.Count + "个词条关联了 " + model.Infor)),
                PerfectionCheckType.Staff => string.IsNullOrWhiteSpace(model.Infor) ? ("需要 填写关联Staff") : (model.Count > 1 ? ("全站约有" + (model.Count - 1).ToString() + "个词条也关联了 " + model.Infor) : ("全站约有" + model.Count + "个词条关联了 " + model.Infor)),
                _ => "需要填写 " + model.CheckType.GetDisplayName(),
            },
        };
    }
}
