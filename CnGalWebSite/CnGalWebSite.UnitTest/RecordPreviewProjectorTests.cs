using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using NUnit.Framework;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class RecordPreviewProjectorTests
{
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
    public void DisambigRelevanceDeletionRemovesTheLoadedRelation()
    {
        var projector = new RecordPreviewProjector(typeof(Disambig));
        projector.Load(new Disambig
        {
            Id = 1,
            Name = "Disambig",
            Entries = new List<Entry> { new() { Id = 7, Name = "Related entry" } }
        });

        projector.Apply(new Examine
        {
            Operation = Operation.DisambigRelevances,
            Context = "{\"Relevances\":[{\"IsDelete\":true,\"EntryId\":7,\"Type\":0}]}"
        });

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
    public void LegacyTagChildDeletionMatchesLoadedChildrenByName()
    {
        var projector = new RecordPreviewProjector(typeof(Tag));
        projector.Load(new Tag
        {
            Id = 1,
            Name = "Tag",
            Entries = new List<Entry> { new() { Id = 9, Name = "Child entry" } }
        });

        projector.Apply(new Examine
        {
            Operation = Operation.EditTag,
            Context = "{\"Name\":\"Tag\",\"ChildrenEntries\":[{\"Name\":\"Child entry\",\"IsDelete\":true}]}"
        });

        var snapshot = projector.Snapshot(value => value);
        Assert.That(snapshot.Items.Any(item => item.Kind == RecordPreviewKind.Relation && item.Value == "entry:9"), Is.False);
    }
}
