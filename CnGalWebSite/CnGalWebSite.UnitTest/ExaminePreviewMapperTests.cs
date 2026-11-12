using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
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
            Audio = [new() { Name = "主题曲", Url = "https://example.com/theme.mp3", BriefIntroduction = "音频说明" }]
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
    public void NullLegacySnapshotIsMarkedIncomplete()
    {
        Assert.That(ExaminePreviewMapper.FromPreData(null).IsComplete, Is.False);
    }
}
