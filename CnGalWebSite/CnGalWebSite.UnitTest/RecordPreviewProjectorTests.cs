using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class RecordPreviewProjectorTests
{
    [TestCase(ExaminedNormalListModelType.Entry, typeof(Entry))]
    [TestCase(ExaminedNormalListModelType.Article, typeof(Article))]
    [TestCase(ExaminedNormalListModelType.Tag, typeof(Tag))]
    [TestCase(ExaminedNormalListModelType.Periphery, typeof(Periphery))]
    [TestCase(ExaminedNormalListModelType.Video, typeof(Video))]
    [TestCase(ExaminedNormalListModelType.User, typeof(ApplicationUser))]
    [TestCase(ExaminedNormalListModelType.PlayedGame, typeof(PlayedGame))]
    [TestCase(ExaminedNormalListModelType.FavoriteFolder, typeof(FavoriteFolder))]
    [TestCase(ExaminedNormalListModelType.Disambig, typeof(Disambig))]
    public void CreateTargetReturnsExpectedAggregate(ExaminedNormalListModelType type, Type expected)
    {
        // This exercises the same factory used by history replay without constructing any
        // repository or application service.
        Assert.That(ExaminePatchApplicationService.CreateEmptyTarget(type), Is.InstanceOf(expected));
    }

    [Test]
    public void CommentSubmissionUsesTheEnumDisplayName()
    {
        var projector = new RecordPreviewProjector(typeof(Comment));
        projector.LoadSubmission(new Examine
        {
            Operation = Operation.PubulishComment,
            Context = "{\"CommentTime\":\"2026-11-12T10:00:00\",\"Type\":2,\"ObjectId\":\"1\",\"Text\":\"hello\"}"
        });

        var type = projector.Snapshot(value => value).Items.Single(item => item.Key == "comment/Type");
        Assert.Multiple(() =>
        {
            Assert.That(type.Value, Is.EqualTo("2"));
            Assert.That(type.DisplayValue, Is.EqualTo(CommentType.ReplyComment.GetDisplayName()));
        });
    }

    [Test]
    public void CertificationSubmissionExposesRelationAndNote()
    {
        var projector = new RecordPreviewProjector(typeof(ApplicationUser));
        projector.LoadSubmission(new Examine
        {
            Operation = Operation.RequestUserCertification,
            Context = "{\"EntryId\":42}",
            Note = "认证说明"
        });

        var snapshot = projector.Snapshot(value => value);
        Assert.Multiple(() =>
        {
            Assert.That(snapshot.Items.Any(item => item.Value == "entry:42"), Is.True);
            Assert.That(snapshot.Items.Any(item => item.Kind == RecordPreviewKind.Body && item.Value == "认证说明"), Is.True);
        });
    }

    [Test]
    public void NestedStaffReleaseAndWebsiteEnumsUseDisplayNames()
    {
        var projector = new RecordPreviewProjector(typeof(Entry));
        projector.Load(new Entry
        {
            Id = 1,
            Name = "Entry",
            EntryStaffFromEntryNavigation = new List<EntryStaff>
            {
                new()
                {
                    ToEntry = 2,
                    Name = "Staff",
                    PositionOfficial = "配音",
                    PositionGeneral = PositionGeneralType.CV
                }
            },
            Releases = new List<GameRelease>
            {
                new()
                {
                    Name = "正式版",
                    PublishPlatformType = PublishPlatformType.Steam,
                    PublishPlatformName = "Steam",
                    Type = GameReleaseType.Official,
                    GamePlatformTypes = new[] { GamePlatformType.Windows, GamePlatformType.Linux },
                    Link = "https://example.com"
                }
            },
            WebsiteAddInfor = new EntryWebsite
            {
                Images = new List<EntryWebsiteImage>
                {
                    new()
                    {
                        Url = "https://example.com/website.png",
                        Type = EntryWebsiteImageType.Carousel,
                        Size = EntryWebsiteImageSize.Large
                    }
                }
            }
        });

        var snapshot = projector.Snapshot(value => value);
        var staff = snapshot.Items.Single(item => item.Group.StartsWith("Staff", StringComparison.Ordinal));
        var release = snapshot.Items.Single(item => item.Group == "发行信息");
        var websiteImage = snapshot.Items.Single(item => item.Group == "官网图片");

        Assert.Multiple(() =>
        {
            Assert.That(staff.Properties.Single(property => property.Key == "PositionGeneral").DisplayValue,
                Is.EqualTo(PositionGeneralType.CV.GetDisplayName()));
            Assert.That(release.Properties.Single(property => property.Key == "PublishPlatformType").DisplayValue,
                Is.EqualTo(PublishPlatformType.Steam.GetDisplayName()));
            Assert.That(release.Properties.Single(property => property.Key == "Type").DisplayValue,
                Is.EqualTo(GameReleaseType.Official.GetDisplayName()));
            Assert.That(release.Properties.Single(property => property.Key == "GamePlatformTypes").DisplayValue,
                Is.EqualTo($"{GamePlatformType.Windows.GetDisplayName()}、{GamePlatformType.Linux.GetDisplayName()}"));
            Assert.That(websiteImage.Properties.Single(property => property.Key == "Type").DisplayValue,
                Is.EqualTo(EntryWebsiteImageType.Carousel.GetDisplayName()));
            Assert.That(websiteImage.Properties.Single(property => property.Key == "Size").DisplayValue,
                Is.EqualTo(EntryWebsiteImageSize.Large.GetDisplayName()));
        });
    }

    [Test]
    public void DefaultStaffPositionGeneralIsNotExposedAsAProperty()
    {
        var projector = new RecordPreviewProjector(typeof(Entry));
        projector.Load(new Entry
        {
            Id = 1,
            Name = "Entry",
            EntryStaffFromEntryNavigation = new List<EntryStaff>
            {
                new()
                {
                    ToEntry = 2,
                    Name = "Staff",
                    PositionOfficial = "配音"
                }
            }
        });

        var staff = projector.Snapshot(value => value).Items
            .Single(item => item.Group.StartsWith("Staff", StringComparison.Ordinal));

        Assert.That(staff.Properties.Any(property => property.Key == "PositionGeneral"), Is.False);
    }

    [Test]
    public void ImageAndAudioThumbnailUrlsUseTheInjectedResolverWithoutChangingCanonicalValues()
    {
        var projector = new RecordPreviewProjector(typeof(Entry), value => "https://img.example/" + value);
        projector.Load(new Entry
        {
            Id = 1,
            Name = "Entry",
            MainPicture = "main.png",
            Pictures = new List<EntryPicture> { new() { Url = "picture.png", Note = "Picture" } },
            Audio = new List<EntryAudio> { new() { Url = "audio.mp3", Name = "Audio", Thumbnail = "thumb.png" } }
        });

        var snapshot = projector.Snapshot(value => value);

        var mainPicture = snapshot.Items.Single(item => item.Key == "main/MainPicture");
        var picture = snapshot.Items.Single(item => item.Kind == RecordPreviewKind.Image && item.Value == "picture.png");
        var audio = snapshot.Items.Single(item => item.Kind == RecordPreviewKind.Audio);
        Assert.Multiple(() =>
        {
            Assert.That(mainPicture.Value, Is.EqualTo("main.png"));
            Assert.That(mainPicture.Url, Is.EqualTo("https://img.example/main.png"));
            Assert.That(picture.Url, Is.EqualTo("https://img.example/picture.png"));
            Assert.That(audio.Url, Is.EqualTo("https://img.example/audio.mp3"));
            Assert.That(audio.Properties.Single(property => property.Key == "Thumbnail").DisplayValue,
                Is.EqualTo("https://img.example/thumb.png"));
        });
    }

    [Test]
    public void DisambigDeletedRelationIsAbsentFromProjectedState()
    {
        var projector = new RecordPreviewProjector(typeof(Disambig));
        projector.Load(new Disambig { Id = 1, Name = "Disambig", Entries = new List<Entry>() });

        var snapshot = projector.Snapshot(value => value);
        Assert.That(snapshot.Items.Any(item => item.Kind == RecordPreviewKind.Relation && item.Value == "entry:7"), Is.False);
    }

    [Test]
    public void PlayedGameRelationsExposeTheAssociatedEntry()
    {
        var projector = new RecordPreviewProjector(typeof(PlayedGame));
        projector.Load(new PlayedGame
        {
            Id = 1,
            Entry = new Entry { Id = 9, Name = "Game", DisplayName = "Game display" }
        });

        var relation = projector.Snapshot(value => value).Items.Single(item => item.Value == "entry:9");
        Assert.That(relation.DisplayValue, Is.EqualTo("Game display"));
        Assert.That(relation.Url, Is.EqualTo("/entries/index/9"));
    }

    [Test]
    public void LoadedTagParentIsProjectedWithItsId()
    {
        var projector = new RecordPreviewProjector(typeof(Tag));
        projector.Load(new Tag
        {
            Id = 1,
            Name = "Tag",
            ParentCodeNavigation = new Tag { Id = 7, Name = "Parent display" }
        });

        var parents = projector.Snapshot(value => value).Items.Where(item => item.Group == "父标签").ToList();
        Assert.Multiple(() =>
        {
            Assert.That(parents, Has.Count.EqualTo(1));
            Assert.That(parents[0].Value, Is.EqualTo("tag:7"));
            Assert.That(parents[0].Url, Is.EqualTo("/tags/index/7"));
        });
    }

    [Test]
    public void LoadedEntryOutlinkIsProjected()
    {
        var projector = new RecordPreviewProjector(typeof(Entry));
        projector.Load(new Entry
        {
            Id = 1,
            Name = "Entry",
            Outlinks = new List<Outlink>
            {
                new() { Name = "Bangumi", BriefIntroduction = "说明", Link = "https://bangumi.tv/subject/316369" }
            }
        });

        var snapshot = projector.Snapshot(value => value);
        var outlink = snapshot.Items.Single(item => item.Kind == RecordPreviewKind.Link);
        Assert.Multiple(() =>
        {
            Assert.That(outlink.Label, Is.EqualTo("Bangumi"));
            Assert.That(outlink.Value, Is.EqualTo("https://bangumi.tv/subject/316369"));
        });
    }

    [Test]
    public void WebsiteImagesWithTheSameUrlButDifferentTypesRemainDistinct()
    {
        var projector = new RecordPreviewProjector(typeof(Entry));
        projector.Load(new Entry
        {
            Id = 1,
            Name = "Entry",
            WebsiteAddInfor = new EntryWebsite
            {
                Images =
                [
                    new() { Url = "https://example.com/website.png", Type = EntryWebsiteImageType.Carousel },
                    new() { Url = "https://example.com/website.png", Type = EntryWebsiteImageType.Background }
                ]
            }
        });

        var images = projector.Snapshot(value => value).Items.Where(item => item.Group == "官网图片").ToList();

        Assert.That(images, Has.Count.EqualTo(2));
    }
}
