using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Admin;
using System.Text.Json;
using System.Text.Json.Nodes;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.Helper.Examines;

/// <summary>
/// A read-only interpreter of the persisted edit instructions. It never attaches entities or
/// invokes the write services. Keys mirror the matching rules used by those services.
/// </summary>
public sealed class RecordPreviewProjector
{
    private readonly Type objectType;
    private readonly Func<string, string> imageUrlResolver;
    private readonly List<RecordPreviewItem> items = new();
    public bool IsComplete { get; set; }
    public bool HasFailed { get; set; }

    private static readonly Dictionary<string, string> Labels = new()
    {
        ["Name"] = "名称", ["DisplayName"] = "显示名称", ["AnotherName"] = "别称",
        ["BriefIntroduction"] = "简介", ["Type"] = "类型", ["Template"] = "样式模板",
        ["MainPicture"] = "主图", ["Thumbnail"] = "缩略图", ["BackgroundPicture"] = "背景图",
        ["SmallBackgroundPicture"] = "小背景图", ["OriginalAuthor"] = "原作者", ["OriginalLink"] = "原文链接",
        ["PubishTime"] = "发布时间", ["RealNewsTime"] = "事件时间", ["NewsType"] = "动态类别",
        ["IsCreatedByCurrentUser"] = "本人创作", ["Copyright"] = "版权", ["Duration"] = "时长",
        ["IsInteractive"] = "互动视频", ["Size"] = "尺寸", ["Category"] = "类别", ["Brand"] = "品牌",
        ["Author"] = "作者", ["Material"] = "材质", ["IndividualParts"] = "部件数量", ["Price"] = "价格",
        ["IsReprint"] = "再版", ["IsAvailableItem"] = "装饰品", ["PageCount"] = "页数", ["SongCount"] = "歌曲数",
        ["SaleLink"] = "贩售链接", ["UserName"] = "用户名", ["PersonalSignature"] = "个性签名",
        ["PhotoPath"] = "头像", ["BackgroundImage"] = "空间头图", ["MBgImage"] = "大背景图", ["SBgImage"] = "小背景图",
        ["MainImage"] = "封面", ["ShowPublicly"] = "是否公开", ["PlayImpressions"] = "评语",
        ["Modifier"] = "分组", ["Note"] = "说明", ["Priority"] = "优先级", ["Url"] = "地址",
        ["PositionOfficial"] = "官方职位", ["PositionGeneral"] = "通用职位", ["CustomName"] = "显示名称",
        ["SubordinateOrganization"] = "隶属组织", ["PublishPlatformType"] = "发行平台类型",
        ["PublishPlatformName"] = "发行平台名称", ["GamePlatformTypes"] = "游戏平台", ["Time"] = "发行时间",
        ["TimeNote"] = "时间备注", ["Engine"] = "引擎", ["Link"] = "链接", ["Target"] = "目标人数",
        ["Open"] = "开放预约", ["IsNeedNotification"] = "预约通知", ["LotteryId"] = "抽奖 ID",
        ["Introduction"] = "介绍", ["Html"] = "自定义 HTML", ["FirstPage"] = "标题", ["SubTitle"] = "副标题",
        ["Impressions"] = "主题语句", ["Color"] = "主题颜色", ["Logo"] = "Logo",
    };

    private static readonly HashSet<string> Images = new()
    { "MainPicture", "Thumbnail", "BackgroundPicture", "SmallBackgroundPicture", "PhotoPath", "BackgroundImage", "MBgImage", "SBgImage", "MainImage", "Logo" };

    private static readonly IReadOnlyDictionary<string, Type> StaffEnumTypes = new Dictionary<string, Type>
    {
        ["PositionGeneral"] = typeof(PositionGeneralType)
    };

    private static readonly IReadOnlyDictionary<string, Type> ReleaseEnumTypes = new Dictionary<string, Type>
    {
        ["PublishPlatformType"] = typeof(PublishPlatformType),
        ["Type"] = typeof(GameReleaseType),
        ["GamePlatformTypes"] = typeof(GamePlatformType[])
    };

    private static readonly IReadOnlyDictionary<string, Type> WebsiteImageEnumTypes = new Dictionary<string, Type>
    {
        ["Type"] = typeof(EntryWebsiteImageType),
        ["Size"] = typeof(EntryWebsiteImageSize)
    };

    /// <summary>
    /// 明确声明可被预览的审核操作。新增 Operation 时该集合必须同步更新，
    /// 并由单元测试保证不会静默遗漏。
    /// </summary>
    public static IReadOnlySet<Operation> SupportedOperations { get; } = new HashSet<Operation>
    {
        Operation.UserMainPage,
        Operation.EstablishMain,
        Operation.EstablishAddInfor,
        Operation.EstablishMainPage,
        Operation.EstablishImages,
        Operation.EstablishRelevances,
        Operation.EstablishTags,
        Operation.EditArticleMain,
        Operation.EditArticleRelevanes,
        Operation.EditArticleMainPage,
        Operation.EditTag,
        Operation.PubulishComment,
        Operation.DisambigMain,
        Operation.DisambigRelevances,
        Operation.EditUserMain,
        Operation.EditPeripheryMain,
        Operation.EditPeripheryImages,
        Operation.EditPeripheryRelatedEntries,
        Operation.EditTagMain,
        Operation.EditTagChildTags,
        Operation.EditTagChildEntries,
        Operation.EditPeripheryRelatedPeripheries,
        Operation.EditPlayedGameMain,
        Operation.EstablishAudio,
        Operation.RequestUserCertification,
        Operation.EditVideoMain,
        Operation.EditVideoImages,
        Operation.EditVideoMainPage,
        Operation.EditVideoRelevanes,
        Operation.EditFavoriteFolderMain,
        Operation.EstablishWebsite,
    };

    public RecordPreviewProjector(Type objectType, Func<string, string> imageUrlResolver = null)
    {
        this.objectType = objectType;
        this.imageUrlResolver = imageUrlResolver;
    }

    public ExaminePreDataModel Snapshot(Func<string, string> render)
    {
        // Every snapshot owns its values; later replay cannot change an earlier version.
        var copy = JsonSerializer.Deserialize<List<RecordPreviewItem>>(JsonSerializer.Serialize(items));
        foreach (var item in copy.Where(i => i.Kind == RecordPreviewKind.Body)) item.Html = render(item.Value ?? "");
        return new() { Items = copy, IsComplete = IsComplete };
    }

    public void Load(object value)
    {
        IsComplete = true;
        HasFailed = false;
        Scalars(Shallow(value), objectType, "main", "主要信息");
        var source = Get(value, "MainPage") ?? Get(value, "MainPageContext");
        if (source is string body) Body(body);
        foreach (var image in List(value, "Pictures")) Media(Shallow(image), false);
        foreach (var audio in List(value, "Audio")) Media(Shallow(audio), true);
        foreach (var info in List(value, "Information")) Information(Shallow(info), List(info, "Additional").Select(Shallow));
        foreach (var staff in List(value, "EntryStaffFromEntryNavigation"))
        {
            var data = Shallow(staff);
            data["StaffId"] = Clone(data["ToEntry"]);
            Staff(data);
        }
        foreach (var release in List(value, "Releases")) Release(Shallow(release));
        foreach (var link in List(value, "Outlinks")) Outlink(Shallow(link), false);
        Relations(value, "Articles", "article", "关联文章");
        Relations(value, "Videos", "video", "关联视频");
        Relations(value, "Tags", "tag", "标签");
        Relations(value, "Entries", "entry", objectType == typeof(Tag) ? "子词条" : "关联词条");
        Relations(value, "RelatedEntries", "entry", "关联词条");
        Relations(value, "InverseParentCodeNavigation", "tag", "子标签");
        foreach (var (property, target, type) in new[] {
            ("EntryRelationFromEntryNavigation", "ToEntry", "entry"),
            ("ArticleRelationFromArticleNavigation", "ToArticle", "article"),
            ("VideoRelationFromVideoNavigation", "ToVideo", "video"),
            ("PeripheryRelationFromPeripheryNavigation", "ToPeriphery", "periphery") })
            foreach (var relation in List(value, property)) Relation(type, Convert.ToString(Get(relation, target), CultureInfo.InvariantCulture), "关联" + RelationLabel(type), false);
        if (Get(value, "ParentCodeNavigation") is { } parent)
            Relation("tag", Convert.ToString(Get(parent, "Id"), CultureInfo.InvariantCulture), "父标签", false, "parent", RelatedDisplayName(parent));
        if (Get(value, "Booking") is { } booking)
        {
            Scalars(Shallow(booking), booking.GetType(), "booking", "预约");
            foreach (var goal in List(booking, "Goals")) Goal(Shallow(goal));
        }
        if (Get(value, "WebsiteAddInfor") is { } website)
        {
            Scalars(Shallow(website), website.GetType(), "website", "官网模板");
            foreach (var image in List(website, "Images")) WebsiteImage(Shallow(image));
        }
        if (objectType == typeof(PlayedGame) && Get(value, "Entry") is { } playedEntry)
        {
            Relation("entry", Convert.ToString(Get(playedEntry, "Id"), CultureInfo.InvariantCulture), "关联词条", false, displayName: RelatedDisplayName(playedEntry));
        }
    }

    public void Apply(Examine record)
    {
        if (!SupportedOperations.Contains(record.Operation))
        {
            throw new InvalidOperationException($"不支持的审核操作：{record.Operation}");
        }

        if (record.Operation is Operation.EstablishMainPage or Operation.EditArticleMainPage or Operation.EditVideoMainPage or Operation.UserMainPage)
        { Body(record.Context); return; }
        var data = JsonNode.Parse(record.Context ?? "{}") as JsonObject ?? new JsonObject();
        switch (record.Operation)
        {
            case Operation.EstablishMain:
            case Operation.EditArticleMain:
            case Operation.EditVideoMain:
            case Operation.EditPeripheryMain:
            case Operation.EditTagMain:
            case Operation.EditUserMain:
            case Operation.EditFavoriteFolderMain:
            case Operation.EditPlayedGameMain:
            case Operation.DisambigMain:
                Scalars(PatchFields(data), objectType, "main", "主要信息");
                if (PatchFields(data)["ParentTagId"] is { } parent)
                {
                    items.RemoveAll(i => i.Group == "父标签");
                    if ((int?)parent > 0) Relation("tag", Text(parent), "父标签", false, "parent");
                }
                break;
            case Operation.EstablishImages:
            case Operation.EditVideoImages:
            case Operation.EditPeripheryImages:
                foreach (var image in Array(data, "Images")) Media(image, false);
                break;
            case Operation.EstablishAudio:
                foreach (var audio in Array(data, "Audio")) Media(audio, true);
                break;
            case Operation.EstablishAddInfor:
                foreach (var info in Array(data, "Information")) Information(info, Array(info, "Additional"));
                foreach (var staff in Array(data, "Staffs")) Staff(staff);
                foreach (var release in Array(data, "Releases")) Release(release);
                if (data["Booking"] is JsonObject booking)
                {
                    Scalars(PatchFields(booking, "MainInfor"), typeof(Booking), "booking", "预约");
                    foreach (var goal in Array(booking, "Goals")) Goal(goal);
                }
                break;
            case Operation.EstablishWebsite:
                Scalars(PatchFields(data, "MainInfor"), typeof(EntryWebsite), "website", "官网模板");
                foreach (var image in Array(data, "Images")) WebsiteImage(image);
                break;
            case Operation.EstablishRelevances:
            case Operation.EditArticleRelevanes:
            case Operation.EditVideoRelevanes:
                foreach (var relation in Array(data, "Relevances"))
                {
                    var type = ((RelevancesType)((int?)relation["Type"] ?? 0)).ToString().ToLowerInvariant();
                    if (type == "outlink") Outlink(relation, true);
                    else Relation(type, Text(relation["DisplayName"]), "关联" + RelationLabel(type), Deleted(relation));
                }
                break;
            case Operation.EstablishTags:
                foreach (var tag in Array(data, "Tags")) Relation("tag", Text(tag["TagId"]), "标签", Deleted(tag));
                break;
            case Operation.EditTagChildTags:
                foreach (var tag in Array(data, "ChildTags")) Relation("tag", Text(tag["TagId"]), "子标签", Deleted(tag));
                break;
            case Operation.EditTagChildEntries:
                foreach (var entry in Array(data, "ChildEntries")) Relation("entry", Text(entry["EntryId"]), "子词条", Deleted(entry));
                break;
            case Operation.EditPeripheryRelatedEntries:
                foreach (var entry in Array(data, "Relevances")) Relation("entry", Text(entry["EntryId"]), "关联词条", Deleted(entry));
                break;
            case Operation.EditPeripheryRelatedPeripheries:
                foreach (var entry in Array(data, "Relevances")) Relation("periphery", Text(entry["PeripheryId"]), "关联周边", Deleted(entry));
                break;
            case Operation.DisambigRelevances:
                foreach (var entry in Array(data, "Relevances"))
                {
                    var type = (int?)entry["Type"] == 1 ? "article" : "entry";
                    Relation(type, Text(entry["EntryId"]), type == "article" ? "关联文章" : "关联词条", Deleted(entry));
                }
                break;
            case Operation.EditTag:
                Scalars(data, typeof(Tag), "main", "主要信息");
                foreach (var tag in Array(data, "ChildrenTags"))
                {
                    var name = Text(tag["Name"]);
                    if (Deleted(tag)) RemoveRelationByName("子标签", name);
                    else if (FindRelationByName("子标签", name) is null) Relation("tag-name", name, "子标签", false);
                }
                foreach (var entry in Array(data, "ChildrenEntries"))
                {
                    var name = Text(entry["Name"]);
                    if (Deleted(entry)) RemoveRelationByName("子词条", name);
                    else if (FindRelationByName("子词条", name) is null) Relation("entry-name", name, "子词条", false);
                }
                if (data.ContainsKey("ParentTag"))
                {
                    var parentName = Text(data["ParentTag"]);
                    var existingParent = FindRelationByName("父标签", parentName);
                    items.RemoveAll(i => i.Group == "父标签" && !ReferenceEquals(i, existingParent));
                    if (!string.IsNullOrWhiteSpace(parentName) && existingParent is null)
                        Relation("tag-name", parentName, "父标签", false, "parent");
                }
                break;
            case Operation.PubulishComment:
                Scalars(new JsonObject { ["Time"] = Clone(data["CommentTime"]), ["Type"] = Clone(data["Type"]), ["ObjectId"] = Clone(data["ObjectId"]) }, null, "comment", "评论",
                    new Dictionary<string, Type> { ["Type"] = typeof(CommentType) });
                Body(Text(data["Text"]));
                break;
            case Operation.RequestUserCertification:
                Relation("entry", Text(data["EntryId"]), "认证词条", false);
                Body(record.Note);
                break;
            default: throw new InvalidOperationException($"不支持的审核操作：{record.Operation}");
        }
    }

    private void Body(string value) => Put(new() { Key = "body", Group = "正文", Kind = RecordPreviewKind.Body, Label = "正文", Value = value ?? "", DisplayValue = value ?? "" });

    private void Scalars(JsonObject data, Type type, string prefix, string group, IReadOnlyDictionary<string, Type> propertyTypes = null)
    {
        foreach (var property in data)
        {
            var propertyName = property.Key;
            if (!Labels.ContainsKey(propertyName) && !(prefix == "comment" && propertyName == "ObjectId")) continue;
            var info = type?.GetProperty(propertyName);
            var propertyType = propertyTypes != null && propertyTypes.TryGetValue(propertyName, out var overrideType)
                ? overrideType
                : info?.PropertyType;
            if (type != null && info == null && (propertyTypes == null || !propertyTypes.ContainsKey(propertyName))) continue;
            var (raw, display) = Format(property.Value, propertyType);
            var kind = Images.Contains(propertyName) ? RecordPreviewKind.Image
                : propertyName is "OriginalLink" or "SaleLink" ? RecordPreviewKind.Link : RecordPreviewKind.Text;
            var url = kind == RecordPreviewKind.Image ? ResolveImage(raw) : raw;
            Put(new() { Key = $"{prefix}/{propertyName}", Group = group, Kind = kind,
                Label = Labels.GetValueOrDefault(propertyName, "目标 ID"), Value = raw,
                DisplayValue = kind == RecordPreviewKind.Image && !string.IsNullOrWhiteSpace(url) ? url : display,
                Url = kind is RecordPreviewKind.Image or RecordPreviewKind.Link ? url : null });
        }
    }

    private void Media(JsonObject data, bool audio)
    {
        var url = Text(data["Url"]);
        var key = Key(audio ? "audio" : "picture", url);
        // Periphery pictures use (group, note, URL) in their persisted write contract.
        if (!audio && objectType == typeof(Periphery)) key = Key("picture", Text(data["Modifier"]), Text(data["Note"]), url);
        var item = new RecordPreviewItem { Key = key, Group = audio ? "音频" : "图片", Kind = audio ? RecordPreviewKind.Audio : RecordPreviewKind.Image,
            Label = Text(data[audio ? "Name" : "Note"]), Value = url, DisplayValue = url, Url = ResolveImage(url),
            Properties = Properties(data, audio ? new[] { "Name", "BriefIntroduction", "Priority", "Duration", "Thumbnail" } : new[] { "Modifier", "Note", "Priority" }) };
        Put(item, Deleted(data));
    }

    private void Information(JsonObject data, IEnumerable<JsonObject> additional)
    {
        var key = Key("information", Text(data["Modifier"]), Text(data["DisplayName"]), Text(data["DisplayValue"]));
        var existing = items.FirstOrDefault(i => i.Key == key);
        var properties = existing?.Properties.ToList() ?? new();
        foreach (var value in additional)
        {
            var name = Text(value["DisplayName"]);
            properties.RemoveAll(p => p.Key == name);
            if (!Deleted(value)) properties.Add(new() { Key = name, Label = name, Value = Text(value["DisplayValue"]), DisplayValue = Text(value["DisplayValue"]) });
        }
        Put(new() { Key = key, Group = "附加信息" + GroupSuffix(data), Kind = RecordPreviewKind.Text, Label = Text(data["DisplayName"]),
            Value = Text(data["DisplayValue"]), DisplayValue = Text(data["DisplayValue"]), Properties = properties }, Deleted(data));
    }

    private void Staff(JsonObject data) => Put(new()
    {
        Key = Key("staff", Text(data["StaffId"]), Text(data["Name"]), Text(data["PositionOfficial"]), Text(data["Modifier"])),
        Group = "Staff" + GroupSuffix(data), Kind = RecordPreviewKind.Text, Label = Text(data["PositionOfficial"]),
        Value = Text(data["StaffId"]) + ":" + Text(data["Name"]), DisplayValue = Text(data["CustomName"]) is { Length: > 0 } name ? name : Text(data["Name"]),
        Properties = StaffProperties(data),
        Url = (int?)data["StaffId"] > 0 ? "/entries/index/" + Text(data["StaffId"]) : null
    }, Deleted(data));

    private void Release(JsonObject data) => Put(new()
    {
        Key = Key("release", Text(data["PublishPlatformType"]), Text(data["PublishPlatformName"]), Text(data["Link"]), Text(data["Name"])),
        Group = "发行信息", Kind = RecordPreviewKind.Text, Label = Text(data["Name"]), Value = Text(data["Name"]), DisplayValue = Text(data["Name"]),
        Properties = Properties(data, ReleaseEnumTypes, "PublishPlatformType", "PublishPlatformName", "Type", "GamePlatformTypes", "Time", "TimeNote", "Engine", "Link")
    }, Deleted(data));

    private void Goal(JsonObject data) => Put(new()
    {
        Key = Key("goal", Text(data["Name"])), Group = "预约目标", Kind = RecordPreviewKind.Text,
        Label = Text(data["Name"]), Value = Text(data["Target"]), DisplayValue = Text(data["Target"])
    }, Deleted(data));

    private void WebsiteImage(JsonObject data) => Put(new()
    {
        Key = Key("website-image", Text(data["Url"])), Group = "官网图片", Kind = RecordPreviewKind.Image,
        Label = Text(data["Note"]), Value = Text(data["Url"]), DisplayValue = Text(data["Url"]), Url = ResolveImage(Text(data["Url"])),
        Properties = Properties(data, WebsiteImageEnumTypes, "Type", "Size", "Note", "Priority")
    }, Deleted(data));

    private void Outlink(JsonObject data, bool patch)
    {
        var name = Text(data[patch ? "DisplayName" : "Name"]);
        var value = Text(data[patch ? "DisplayValue" : "BriefIntroduction"]);
        Put(new() { Key = Key("outlink", name), Group = "外链", Kind = RecordPreviewKind.Link, Label = name,
            Value = Text(data["Link"]), Url = Text(data["Link"]), DisplayValue = value,
            Properties = new() { new() { Key = "description", Label = "说明", Value = value, DisplayValue = value } } }, Deleted(data));
    }

    private void Relations(object value, string property, string type, string group)
    {
        foreach (var related in List(value, property))
        {
            Relation(type, Convert.ToString(Get(related, "Id"), CultureInfo.InvariantCulture), group, false, displayName: RelatedDisplayName(related));
        }
    }

    private void Relation(string type, string id, string group, bool deleted, string slot = null, string displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        var fallbackLabel = RelationLabel(type) + " #" + id;
        var label = string.IsNullOrWhiteSpace(displayName) ? fallbackLabel : displayName;
        Put(new() { Key = Key("relation", slot ?? group, type, id), Group = group, Kind = RecordPreviewKind.Relation,
            Label = label, Value = type + ":" + id, DisplayValue = label,
            Url = type switch { "entry" => "/entries/index/" + id, "article" => "/articles/index/" + id,
                "video" => "/videos/index/" + id, "tag" => "/tags/index/" + id, "periphery" => "/peripheries/index/" + id, _ => null } }, deleted);
    }

    private RecordPreviewItem FindRelationByName(string group, string name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : items.FirstOrDefault(i => i.Kind == RecordPreviewKind.Relation
            && i.Group == group && (string.Equals(i.DisplayValue, name, StringComparison.Ordinal) || string.Equals(i.Label, name, StringComparison.Ordinal)));
    }

    private void RemoveRelationByName(string group, string name)
    {
        var item = FindRelationByName(group, name);
        if (item is not null) items.Remove(item);
    }

    private string ResolveImage(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : imageUrlResolver?.Invoke(value) ?? value;

    private static string RelatedDisplayName(object related)
    {
        var displayName = Convert.ToString(Get(related, "DisplayName"), CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(displayName)
            ? Convert.ToString(Get(related, "Name"), CultureInfo.InvariantCulture)
            : displayName;
    }

    private void Put(RecordPreviewItem item, bool deleted = false)
    {
        var index = items.FindIndex(i => i.Key == item.Key);
        if (deleted)
        {
            if (index >= 0) items.RemoveAt(index);
        }
        else if (index >= 0) items[index] = item;
        else items.Add(item);
    }

    private List<RecordPreviewProperty> Properties(JsonObject data, params string[] names) =>
        Properties(data, null, names);

    private List<RecordPreviewProperty> StaffProperties(JsonObject data)
    {
        var properties = Properties(data, StaffEnumTypes, "CustomName", "PositionGeneral", "SubordinateOrganization");
        // 通用职位是非空枚举，默认值 Other(0) 在旧编辑数据中没有实际含义。
        if (Text(data["PositionGeneral"]) is "" or "0")
        {
            properties.RemoveAll(property => property.Key == "PositionGeneral");
        }
        return properties;
    }

    private List<RecordPreviewProperty> Properties(JsonObject data, IReadOnlyDictionary<string, Type> propertyTypes, params string[] names) => names.Select(name =>
    {
        var propertyType = propertyTypes != null && propertyTypes.TryGetValue(name, out var overrideType) ? overrideType : null;
        var (raw, display) = Format(data[name], propertyType);
        return new RecordPreviewProperty { Key = name, Label = Labels.GetValueOrDefault(name, name), Value = raw,
            DisplayValue = name == "Thumbnail" ? ResolveImage(display) : display };
    }).Where(property => !string.IsNullOrWhiteSpace(property.Value) || !string.IsNullOrWhiteSpace(property.DisplayValue)).ToList();

    private static (string Raw, string Display) Format(JsonNode value, Type type)
    {
        if (value is null) return ("", "");
        try
        {
            type = Nullable.GetUnderlyingType(type ?? typeof(string)) ?? type;
            object converted = type != null
                ? JsonSerializer.Deserialize(value.ToJsonString(), type)
                : value is JsonValue scalar && scalar.TryGetValue<object>(out var scalarValue) ? scalarValue : null;
            if (converted is Enum enumeration) return (Convert.ToInt64(enumeration).ToString(CultureInfo.InvariantCulture), enumeration.GetDisplayName());
            if (converted is IEnumerable enumerable && converted is not string)
            {
                var values = enumerable.Cast<object>().ToList();
                if (values.Count > 0 && values.All(item => item is Enum))
                    return (Text(value), string.Join("、", values.Cast<Enum>().Select(item => item.GetDisplayName())));
            }
            if (converted is bool boolean) return (boolean ? "true" : "false", boolean ? "是" : "否");
            if (converted is DateTime date) return (date.ToUniversalTime().ToString("O"), date.ToString("yyyy-MM-dd HH:mm:ss"));
            if (converted is TimeSpan duration) return (duration.ToString("c"), duration.ToString("c"));
        }
        catch (Exception e) when (e is FormatException or ArgumentException or JsonException or InvalidCastException or OverflowException or NotSupportedException) { }
        var text = Text(value);
        return (text, text);
    }

    private static JsonObject PatchFields(JsonObject data, string property = "Items") => data[property] is JsonArray fields
        ? new JsonObject(fields.OfType<JsonObject>().Where(f => !string.IsNullOrEmpty(Text(f["Key"])))
            .GroupBy(f => Text(f["Key"])).Select(g => new KeyValuePair<string, JsonNode>(g.Key, Clone(g.Last()["Value"])))) : data;
    private static string Text(JsonNode value)
    {
        if (value is null) return "";
        if (value is JsonValue scalar)
        {
            return scalar.TryGetValue<string>(out var text) ? text : scalar.ToJsonString();
        }
        return value.ToJsonString();
    }
    private static bool Deleted(JsonObject value) => value?["IsDelete"] is JsonValue deleted
        && deleted.TryGetValue<bool>(out var isDeleted) && isDeleted;
    private static IEnumerable<JsonObject> Array(JsonObject value, string property) =>
        (value[property] as JsonArray)?.OfType<JsonObject>() ?? Enumerable.Empty<JsonObject>();
    private static string Key(string prefix, params string[] values) => prefix + "/" + JsonSerializer.Serialize(values);
    private static string GroupSuffix(JsonObject data) => string.IsNullOrWhiteSpace(Text(data["Modifier"])) ? "" : " · " + Text(data["Modifier"]);
    private static object Get(object value, string property) => value?.GetType().GetProperty(property)?.GetValue(value);
    private static IEnumerable<object> List(object value, string property) => (Get(value, property) as IEnumerable)?.Cast<object>() ?? Enumerable.Empty<object>();
    private static JsonNode Clone(JsonNode value) => value is null ? null : JsonNode.Parse(value.ToJsonString());
    private static JsonObject Shallow(object value)
    {
        var result = new JsonObject();
        foreach (var property in value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && (p.PropertyType.IsValueType || p.PropertyType == typeof(string) || p.PropertyType.IsArray)))
        {
            result[property.Name] = JsonSerializer.SerializeToNode(property.GetValue(value));
        }
        return result;
    }
    private static string RelationLabel(string type) => type switch
    { "entry" or "entry-name" => "词条", "article" => "文章", "video" => "视频", "tag" or "tag-name" => "标签", "periphery" => "周边", _ => "对象" };
}
