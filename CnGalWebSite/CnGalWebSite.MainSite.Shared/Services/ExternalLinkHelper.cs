using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 外部链接相关的共享帮助方法，供多个详情页复用。
/// </summary>
public static class ExternalLinkHelper
{
    private const string ImageBaseUrl = "https://res.cngal.org/_content/CnGalWebSite.Shared/images/";

    public static bool IsExternalLink(string? link)
    {
        return string.IsNullOrWhiteSpace(link) is false
               && (link.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                   || link.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }

    public static string GetArticleLink(ArticleInforTipViewModel item)
    {
        return IsExternalLink(item.Link) ? item.Link! : $"/articles/index/{item.Id}";
    }

    public static string? GetTarget(string? link)
    {
        return IsExternalLink(link) ? "_blank" : null;
    }

    public static string DisplayTextOrFallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    public static string GetExternalLinkIcon(string displayName)
    {
        var name = displayName.ToLower();
        if (name.Contains("steam")) return "mdiSteam";
        if (name.Contains("bilibili") || name.Contains("b站")) return "mdiTelevisionPlay";
        if (name.Contains("weibo") || name.Contains("微博")) return "mdiWechat";
        if (name.Contains("twitter") || name.Contains("x")) return "mdiTwitter";
        if (name.Contains("github")) return "mdiGithub";
        if (name.Contains("youtube")) return "mdiYoutube";
        if (name.Contains("tap") || name.Contains("taptap")) return "mdiGamepadVariantOutline";
        if (name.Contains("qq")) return "mdiQqchat";
        return "mdiEarth";
    }

    public static string? GetExternalLinkImage(string displayName)
    {
        var image = displayName switch
        {
            "萌娘百科" => "Moegirl.png",
            "Bangumi" => "Bangumi.png",
            "百度百科" => "BaiDuWiki.png",
            "2DFan" => "2DFan.png",
            "中文维基百科" => "Wiki.png",
            "月幕Galgame" => "YMGal.png",
            "Bilibili" => "bilibili.png",
            "bilibili" => "bilibili.png",
            "WikiData" => "Wikidata.png",
            "微博" => "weibo.png",
            "AcFun" => "AcFun.png",
            "知乎" => "zhihu.png",
            "爱发电" => "Afdian.png",
            "Pixiv" => "pixiv.png",
            "Twitter" => "twitter.png",
            "YouTube" => "Youtube.png",
            "Facebook" => "Facebook.png",
            "官网" => "SmartHome.png",
            "摩点" => "modian.png",
            "小黑盒" => "xiaoheihe.jpg",
            _ => null
        };

        if (string.IsNullOrWhiteSpace(image))
        {
            return null;
        }

        return ImageBaseUrl + image;
    }

    public static string? GetExternalLinkDescription(RelevancesKeyValueModel item)
    {
        return item.DisplayName switch
        {
            "VNDB" => item.DisplayValue + (string.IsNullOrWhiteSpace(item.DisplayValue) ? "" : " - ") + "VNDB.org力争成为一个关于视觉小说的全面的信息数据库",
            _ => item.DisplayValue
        };
    }
}
