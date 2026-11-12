using System.Globalization;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>Explicit projections keep historical content separate from live counters and permissions.</summary>
public static class EditRecordContentMapper
{
    public static ExaminePreDataModel Map(EntryIndexViewModel value)
    {
        var result = Create(value.MainPage,
            ("名称", value.Name), ("别称", value.AnotherName), ("简介", value.BriefIntroduction),
            ("类型", value.Type), ("样式模板", value.Template), ("配音", value.IsDubbing));
        Images(result, ("主图", value.MainPicture), ("缩略图", value.Thumbnail),
            ("背景图", value.BackgroundPicture), ("小背景图", value.SmallBackgroundPicture));
        Pictures(result, value.Pictures);
        Group(result, "附加信息", (value.Information ?? []).Select(i => (i.Name, (object?)i.Value)));
        Group(result, "制作组", (value.ProductionGroups ?? []).Select(i => ("名称", (object?)i.DisplayName)));
        Group(result, "发行商", (value.Publishers ?? []).Select(i => ("名称", (object?)i.DisplayName)));
        foreach (var staff in value.Staffs ?? [])
            Group(result, $"Staff · {staff.Modifier}", (staff.StaffList ?? []).SelectMany(position =>
                (position.Names ?? []).Select(name => (position.Modifier, (object?)name.DisplayName))));
        foreach (var release in value.Releases ?? [])
        {
            Group(result, $"发行信息 · {release.Name}", [("发行平台", release.PublishPlatformType),
                ("平台名称", release.PublishPlatformName), ("类别", release.Type), ("发行时间", release.Time),
                ("时间备注", release.TimeNote), ("引擎", release.Engine),
                ("游戏平台", string.Join("、", (release.GamePlatformTypes ?? []).Select(p => p.GetDisplayName())))]);
            Link(result, release.Name ?? "发行链接", release.GetLink());
        }
        if (value.Booking is { } booking)
            Group(result, "预约", new[] { ("开放预约", (object?)booking.Open) }.Concat(
                (booking.Goals ?? []).Select(g => (g.Name, (object?)g.Target))));
        if (value.WebsiteAddInfor is { } website)
        {
            // Custom website HTML is displayed as text, never inserted into the document.
            Group(result, "官网补充信息", [("介绍", website.Introduction), ("自定义 HTML", website.Html),
                ("标题", website.FirstPage), ("副标题", website.SubTitle), ("主题语句", website.Impressions), ("主题颜色", website.Color)]);
            Images(result, ("官网 Logo", website.Logo));
            foreach (var image in website.Images ?? []) Images(result, ($"官网 · {image.Note}", image.Url));
        }
        result.Audio = value.Audio ?? [];
        result.Outlinks.AddRange(value.OtherRelevances ?? []);
        result.Relevances.AddRange((value.EntryRelevances ?? []).Select(i => new SearchAloneModel { entry = i }));
        result.Relevances.AddRange((value.ArticleRelevances ?? []).Select(i => new SearchAloneModel { article = i }));
        result.Relevances.AddRange((value.VideoRelevances ?? []).Select(i => new SearchAloneModel { video = i }));
        result.Relevances.AddRange((value.Roles ?? []).Select(i => new SearchAloneModel { entry = i }));
        result.Relevances.AddRange((value.StaffGames ?? []).Select(i => new SearchAloneModel { entry = i }));
        result.Relevances.AddRange((value.Tags ?? []).Select(i => new SearchAloneModel
            { tag = new TagInforTipViewModel { Id = i.Id, Name = i.Name } }));
        return result;
    }

    public static ExaminePreDataModel Map(ArticleViewModel value)
    {
        var result = Create(value.MainPage, ("名称", value.Name), ("显示名称", value.DisplayName),
            ("简介", value.BriefIntroduction), ("类型", value.Type), ("原作者", value.OriginalAuthor), ("发布时间", value.PubishTime),
            ("真实动态时间", value.RealNewsTime), ("动态类型", value.NewsType), ("用户创作", value.IsCreatedByCurrentUser));
        Images(result, ("主图", value.MainPicture), ("背景图", value.BackgroundPicture), ("小背景图", value.SmallBackgroundPicture));
        Link(result, "原文链接", value.OriginalLink);
        result.Outlinks.AddRange(value.RelatedOutlinks ?? []);
        result.Relevances.AddRange((value.RelatedEntries ?? []).Select(i => new SearchAloneModel { entry = i }));
        result.Relevances.AddRange((value.RelatedArticles ?? []).Select(i => new SearchAloneModel { article = i }));
        result.Relevances.AddRange((value.RelatedVideos ?? []).Select(i => new SearchAloneModel { video = i }));
        return result;
    }

    public static ExaminePreDataModel Map(VideoViewModel value)
    {
        var result = Create(value.MainPage, ("名称", value.Name), ("显示名称", value.DisplayName),
            ("简介", value.BriefIntroduction), ("类型", value.Type), ("版权", value.Copyright),
            ("时长", value.Duration), ("互动视频", value.IsInteractive), ("原作者", value.OriginalAuthor), ("发布时间", value.PubishTime));
        Images(result, ("主图", value.MainPicture), ("背景图", value.BackgroundPicture), ("小背景图", value.SmallBackgroundPicture));
        Pictures(result, value.Pictures);
        result.Outlinks.AddRange(value.RelatedOutlinks ?? []);
        result.Relevances.AddRange((value.RelatedEntries ?? []).Select(i => new SearchAloneModel { entry = i }));
        result.Relevances.AddRange((value.RelatedArticles ?? []).Select(i => new SearchAloneModel { article = i }));
        result.Relevances.AddRange((value.RelatedVideos ?? []).Select(i => new SearchAloneModel { video = i }));
        return result;
    }

    public static ExaminePreDataModel Map(TagIndexViewModel value)
    {
        var result = Create(null, ("名称", value.Name), ("简介", value.BriefIntroduction));
        Images(result, ("主图", value.MainPicture), ("缩略图", value.Thumbnail),
            ("背景图", value.BackgroundPicture), ("小背景图", value.SmallBackgroundPicture));
        if (value.ParentTag is { Id: > 0 } parent) result.Relevances.Add(new() { tag = parent });
        result.Relevances.AddRange((value.ChildrenTags ?? []).Select(i => new SearchAloneModel { tag = i }));
        result.Relevances.AddRange((value.ChildrenEntries ?? []).Select(i => new SearchAloneModel { entry = i }));
        return result;
    }

    public static ExaminePreDataModel Map(PeripheryViewModel value)
    {
        var result = Create(null, ("名称", value.Name), ("显示名称", value.DisplayName), ("简介", value.BriefIntroduction),
            ("类型", value.Type), ("尺寸", value.Size), ("类别", value.Category), ("品牌", value.Brand),
            ("作者", value.Author), ("材质", value.Material), ("部件数量", value.IndividualParts),
            ("价格", value.Price), ("再版", value.IsReprint), ("装饰品", value.IsAvailableItem),
            ("页数", value.PageCount), ("歌曲数", value.SongCount));
        Images(result, ("主图", value.MainPicture), ("缩略图", value.Thumbnail),
            ("背景图", value.BackgroundPicture), ("小背景图", value.SmallBackgroundPicture));
        Pictures(result, value.Pictures);
        Link(result, "贩售链接", value.SaleLink);
        result.Relevances.AddRange((value.Entries ?? []).Select(i => new SearchAloneModel { entry = i }));
        result.Relevances.AddRange((value.Peripheries ?? []).Select(i => new SearchAloneModel { periphery = i }));
        return result;
    }

    private static ExaminePreDataModel Create(string? mainPage, params (string Name, object? Value)[] fields)
    {
        var result = new ExaminePreDataModel { MainPage = mainPage! };
        Group(result, "主要信息", fields);
        return result;
    }

    private static void Group(ExaminePreDataModel result, string name, IEnumerable<(string Name, object? Value)> fields)
    {
        var values = fields.Where(f => f.Value is not null && (f.Value is not string text || !string.IsNullOrWhiteSpace(text)))
            .Select(f => new KeyValueModel { DisplayName = f.Name, DisplayValue = f.Value switch
            {
                bool b => b ? "是" : "否", Enum e => e.GetDisplayName(),
                DateTime date => date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                _ => Convert.ToString(f.Value, CultureInfo.InvariantCulture) ?? ""
            }}).ToList();
        if (values.Count > 0) result.Texts.Add(new() { Modifier = name, Informations = values });
    }

    private static void Images(ExaminePreDataModel result, params (string Name, string? Url)[] images)
    {
        foreach (var image in images.Where(i => !string.IsNullOrWhiteSpace(i.Url)))
            result.Pictures.Add(new() { Note = image.Name, Url = image.Url! });
    }

    private static void Pictures(ExaminePreDataModel result, IEnumerable<PicturesViewModel>? groups)
    {
        foreach (var group in groups ?? [])
            foreach (var image in group.Pictures ?? [])
                result.Pictures.Add(new() { Url = image.Url, Note = string.Join(" · ",
                    new[] { group.Modifier, image.Note }.Where(s => !string.IsNullOrWhiteSpace(s))), Priority = image.Priority });
    }

    private static void Link(ExaminePreDataModel result, string name, string? url)
    {
        if (!string.IsNullOrWhiteSpace(url)) result.Outlinks.Add(new() { DisplayName = name, DisplayValue = url, Link = url });
    }
}
