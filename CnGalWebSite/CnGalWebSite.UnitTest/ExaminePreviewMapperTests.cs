using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Entries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.Extensions;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class ExaminePreviewMapperTests
{
    [Test]
    public void LegacyPreviewPayloadIsMappedToClientRecordItems()
    {
        var legacy = new ExaminePreDataModel
        {
            MainPage = "<p>正文</p>",
            Pictures = [new() { Note = "主图", Url = "https://example.com/a.png", Priority = 10 }],
            Texts =
            [
                new()
                {
                    Modifier = "主要信息",
                    Informations =
                    [
                        new() { DisplayName = "名称", DisplayValue = "测试词条" },
                        new() { DisplayName = "类型", DisplayValue = "游戏" }
                    ]
                }
            ],
            Relevances =
            [
                new() { entry = new EntryInforTipViewModel { Id = 42, Name = "关联词条" } }
            ],
            Outlinks = [new() { DisplayName = "官网", DisplayValue = "官网说明", Link = "https://example.com" }],
            Audio = [new() { Name = "主题曲", Url = "https://example.com/theme.mp3", BriefIntroduction = "音频说明",
                Duration = TimeSpan.FromMinutes(3), Priority = 2 }]
        };

        var result = ExaminePreviewMapper.FromPreData(legacy);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsComplete, Is.True);
            Assert.That(result.Items.Single(i => i.Kind == RecordPreviewKind.Body).Value, Is.EqualTo("<p>正文</p>"));
            Assert.That(result.Items.Single(i => i.Kind == RecordPreviewKind.Image).Url, Is.EqualTo("https://example.com/a.png"));
            Assert.That(result.Items.Where(i => i.Kind == RecordPreviewKind.Text).Select(i => i.DisplayValue),
                Is.EquivalentTo(new[] { "测试词条", "游戏" }));
            Assert.That(result.Items.Single(i => i.Kind == RecordPreviewKind.Relation).Value, Is.EqualTo("entry:42"));
            Assert.That(result.Items.Single(i => i.Kind == RecordPreviewKind.Link).Url, Is.EqualTo("https://example.com"));
            Assert.That(result.Items.Single(i => i.Kind == RecordPreviewKind.Audio).Url, Is.EqualTo("https://example.com/theme.mp3"));
            var audioProperties = result.Items.Single(i => i.Kind == RecordPreviewKind.Audio).Properties
                .ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
            Assert.That(audioProperties["Duration"], Is.EqualTo(TimeSpan.FromMinutes(3).ToString()));
            Assert.That(audioProperties["Priority"], Is.EqualTo("2"));
        });
    }

    [Test]
    public void ArticleNewsFieldsParticipateInTheSnapshot()
    {
        var legacy = EditRecordContentMapper.Map(new ArticleViewModel
        {
            Name = "动态标题",
            MainPage = "<p>正文</p>",
            NewsType = "公告",
            RealNewsTime = new DateTime(2026, 11, 12, 9, 30, 0),
            PubishTime = new DateTime(2026, 11, 11, 8, 0, 0),
            IsCreatedByCurrentUser = true
        });

        var values = legacy.Texts.SelectMany(t => t.Informations)
            .ToDictionary(i => i.DisplayName, i => i.DisplayValue, StringComparer.Ordinal);
        Assert.Multiple(() =>
        {
            Assert.That(values["动态类型"], Is.EqualTo("公告"));
            Assert.That(values["真实动态时间"], Is.EqualTo("2026-11-12 09:30"));
            Assert.That(values["用户创作"], Is.EqualTo("是"));
        });
    }

    [Test]
    public void DomainHistoryViewModelIsMappedThroughTheLegacyProjection()
    {
        var legacy = EditRecordContentMapper.Map(new EntryIndexViewModel
        {
            Name = "历史词条",
            MainPage = "<p>历史正文</p>",
            MainPicture = "main.png",
            Tags = [new() { Id = 3, Name = "历史标签" }]
        });

        var result = ExaminePreviewMapper.FromPreData(legacy);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Any(i => i.Kind == RecordPreviewKind.Body && i.Value == "<p>历史正文</p>"), Is.True);
            Assert.That(result.Items.Any(i => i.Kind == RecordPreviewKind.Image && i.Value == "main.png"), Is.True);
            Assert.That(result.Items.Any(i => i.Kind == RecordPreviewKind.Relation && i.Value == "tag:3"), Is.True);
        });
    }

    [Test]
    public void EntryNewsAndFullInformationParticipateInComparison()
    {
        var before = EditRecordContentMapper.Map(new EntryIndexViewModel
        {
            Name = "高考恋爱一百天",
            NewsOfEntry =
            [
                new() { ArticleId = 1155, Title = "要上考场的同学们", Link = "https://weibo.com/old", HappenedTime = new(2022, 6, 6) }
            ],
            AllInformation =
            [
                new() { Modifier = "基本信息", DisplayName = "发行时间", DisplayValue = "2014年7月20日" }
            ]
        });
        var after = EditRecordContentMapper.Map(new EntryIndexViewModel
        {
            Name = "高考恋爱一百天",
            NewsOfEntry =
            [
                new() { ArticleId = 2613, Title = "这游戏做了8年", Link = "https://www.bilibili.com/video/new", HappenedTime = new(2023, 5, 25) },
                new() { ArticleId = 1155, Title = "要上考场的同学们", Link = "https://weibo.com/old", HappenedTime = new(2022, 6, 6) }
            ]
        });

        var diff = RecordDiffBuilder.Build(ExaminePreviewMapper.FromPreData(before), ExaminePreviewMapper.FromPreData(after));

        Assert.Multiple(() =>
        {
            Assert.That(diff.Items.Any(item => item.Item.Group == "关联动态" && item.Item.Label == "这游戏做了8年"), Is.True);
            Assert.That(diff.Items.Any(item => item.Item.Group == "基本信息" && item.Item.Label == "发行时间" && item.Change == RecordChangeKind.Removed), Is.True);
        });
    }

    [Test]
    public void EmptyStaffModifierDoesNotLeaveTrailingSeparator()
    {
        var legacy = EditRecordContentMapper.Map(new EntryIndexViewModel
        {
            Name = "测试词条",
            Staffs =
            [
                new()
                {
                    Modifier = "",
                    StaffList =
                    [
                        new() { Modifier = "剧本", Names = [new() { DisplayName = "测试 Staff" }] }
                    ]
                }
            ]
        });

        Assert.That(legacy.Texts.Any(text => text.Modifier == "Staff"), Is.True);
        Assert.That(legacy.Texts.Any(text => text.Modifier.StartsWith("Staff ·", StringComparison.Ordinal)), Is.False);
    }

    [Test]
    public void DuplicateTextGroupModifiersProduceDistinctKeys()
    {
        var legacy = new ExaminePreDataModel
        {
            Texts =
            [
                new()
                {
                    Modifier = "发行信息 · 同一名称",
                    Informations = [new() { DisplayName = "发行平台", DisplayValue = "Steam" }]
                },
                new()
                {
                    Modifier = "发行信息 · 同一名称",
                    Informations = [new() { DisplayName = "发行平台", DisplayValue = "百度贴吧" }]
                }
            ]
        };

        var result = ExaminePreviewMapper.FromPreData(legacy);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.Items.Select(item => item.Key).Distinct().Count(), Is.EqualTo(2));
        });
    }

    [Test]
    public void ReleasesWithSameNameAreSeparatedByPlatform()
    {
        var legacy = EditRecordContentMapper.Map(new EntryIndexViewModel
        {
            Name = "关于我被小学女生绑架这件事",
            Releases =
            [
                new()
                {
                    Name = "关于我被小学女生绑架这件事",
                    PublishPlatformType = PublishPlatformType.Steam,
                    Type = GameReleaseType.Official,
                    GamePlatformTypes = [GamePlatformType.Windows],
                    Time = new DateTime(2018, 5, 31),
                    Engine = "Ren'Py",
                    Link = "862690"
                },
                new()
                {
                    Name = "关于我被小学女生绑架这件事",
                    PublishPlatformType = PublishPlatformType.Other,
                    PublishPlatformName = "百度贴吧",
                    Type = GameReleaseType.Official,
                    GamePlatformTypes = [GamePlatformType.Windows],
                    Time = new DateTime(2017, 5, 31),
                    Engine = "Unity(Utage)",
                    Link = "https://tieba.baidu.com/p/5142940840"
                }
            ]
        });

        var result = ExaminePreviewMapper.FromPreData(legacy);
        var groups = result.Items
            .Where(item => item.Group.StartsWith("发行信息", StringComparison.Ordinal))
            .Select(item => item.Group)
            .Distinct()
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(groups, Has.Count.EqualTo(2));
            Assert.That(groups.Any(group => group.Contains("Steam", StringComparison.Ordinal)), Is.True);
            Assert.That(groups.Any(group => group.Contains("百度贴吧", StringComparison.Ordinal)), Is.True);
        });
    }

    [Test]
    public void NullLegacySnapshotIsMarkedIncomplete()
    {
        Assert.That(ExaminePreviewMapper.FromPreData(null).IsComplete, Is.False);
    }
}
