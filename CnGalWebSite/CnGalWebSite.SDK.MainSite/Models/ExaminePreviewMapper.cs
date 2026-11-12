using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// Converts the legacy API preview payloads into the client-side record model used by the
/// new review UI. All diffing and presentation stay in the frontend.
/// </summary>
public static class ExaminePreviewMapper
{
    public static RecordPreviewModel FromPreData(ExaminePreDataModel? value)
    {
        var result = new RecordPreviewModel { IsComplete = true };
        if (value is null) return new RecordPreviewModel { IsComplete = false };

        if (!string.IsNullOrWhiteSpace(value.MainPage))
        {
            result.Items.Add(new RecordPreviewItem
            {
                Key = "body", Group = "正文", Kind = RecordPreviewKind.Body,
                Label = "正文", Value = value.MainPage, DisplayValue = value.MainPage
            });
        }

        for (var index = 0; index < value.Pictures.Count; index++)
        {
            var picture = value.Pictures[index];
            if (string.IsNullOrWhiteSpace(picture.Url)) continue;
            result.Items.Add(new RecordPreviewItem
            {
                Key = $"picture/{picture.Note}/{picture.Url}", Group = "图片",
                Kind = RecordPreviewKind.Image, Label = picture.Note ?? string.Empty,
                Value = picture.Url, DisplayValue = picture.Url, Url = picture.Url,
                Properties = [new() { Key = "Priority", Label = "优先级", Value = picture.Priority.ToString(), DisplayValue = picture.Priority.ToString() }]
            });
        }

        foreach (var group in value.Texts)
        {
            var nameCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var information in group.Informations)
            {
                if (string.IsNullOrWhiteSpace(information.DisplayValue)) continue;
                var label = information.DisplayName ?? string.Empty;
                nameCounts.TryGetValue(label, out var occurrence);
                nameCounts[label] = occurrence + 1;
                result.Items.Add(new RecordPreviewItem
                {
                    Key = $"text/{group.Modifier}/{label}/{occurrence}",
                    Group = string.IsNullOrWhiteSpace(group.Modifier) ? "文本信息" : group.Modifier,
                    Kind = RecordPreviewKind.Text, Label = label,
                    Value = information.DisplayValue, DisplayValue = information.DisplayValue
                });
            }
        }

        foreach (var relevance in value.Relevances)
        {
            var item = relevance.entry is not null
                ? Relation("entry", relevance.entry.Id.ToString(), relevance.entry.Name)
                : relevance.article is not null
                    ? Relation("article", relevance.article.Id.ToString(), FirstNonEmpty(relevance.article.DisplayName, relevance.article.Name))
                    : relevance.video is not null
                        ? Relation("video", relevance.video.Id.ToString(), FirstNonEmpty(relevance.video.DisplayName, relevance.video.Name))
                        : relevance.tag is not null
                            ? Relation("tag", relevance.tag.Id.ToString(), relevance.tag.Name)
                            : relevance.periphery is not null
                                ? Relation("periphery", relevance.periphery.Id.ToString(), relevance.periphery.Name)
                                : relevance.user is not null
                                    ? Relation("user", relevance.user.Id, relevance.user.UserName)
                                    : null;
            if (item is not null) result.Items.Add(item);
        }

        foreach (var link in value.Outlinks)
        {
            if (string.IsNullOrWhiteSpace(link.Link)) continue;
            result.Items.Add(new RecordPreviewItem
            {
                Key = $"link/{link.DisplayName}",
                Group = "外链", Kind = RecordPreviewKind.Link,
                Label = link.DisplayName ?? string.Empty,
                Value = link.Link,
                DisplayValue = string.IsNullOrWhiteSpace(link.DisplayValue) ? link.Link : link.DisplayValue,
                Url = link.Link
            });
        }

        for (var index = 0; index < value.Audio.Count; index++)
        {
            var audio = value.Audio[index];
            if (string.IsNullOrWhiteSpace(audio.Url)) continue;
            result.Items.Add(new RecordPreviewItem
            {
                Key = $"audio/{audio.Name}/{index}", Group = "音频",
                Kind = RecordPreviewKind.Audio, Label = audio.Name ?? string.Empty,
                Value = audio.Url, DisplayValue = audio.Url, Url = audio.Url,
                Properties =
                [
                    new() { Key = "Name", Label = "名称", Value = audio.Name ?? string.Empty, DisplayValue = audio.Name ?? string.Empty },
                    new() { Key = "BriefIntroduction", Label = "简介", Value = audio.BriefIntroduction ?? string.Empty, DisplayValue = audio.BriefIntroduction ?? string.Empty },
                    new() { Key = "Duration", Label = "时长", Value = audio.Duration.ToString(), DisplayValue = audio.Duration.ToString() },
                    new() { Key = "Priority", Label = "优先级", Value = audio.Priority.ToString(), DisplayValue = audio.Priority.ToString() },
                    new() { Key = "Thumbnail", Label = "缩略图", Value = audio.Thumbnail ?? string.Empty, DisplayValue = audio.Thumbnail ?? string.Empty }
                ]
            });
        }

        return result;
    }

    private static RecordPreviewItem Relation(string type, string id, string? name)
    {
        var display = string.IsNullOrWhiteSpace(name) ? id : name;
        return new RecordPreviewItem
        {
            Key = $"relation/{type}/{id}", Group = "关联内容", Kind = RecordPreviewKind.Relation,
            Label = display, Value = type + ":" + id, DisplayValue = display,
            Url = type switch
            {
                "entry" => "/entries/index/" + id,
                "article" => "/articles/index/" + id,
                "video" => "/videos/index/" + id,
                "tag" => "/tags/index/" + id,
                "periphery" => "/peripheries/index/" + id,
                "user" => "/space/index/" + Uri.EscapeDataString(id),
                _ => null
            }
        };
    }

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
}
