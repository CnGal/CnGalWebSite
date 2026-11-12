using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace CnGalWebSite.UnitTest;

[TestFixture]
public class ExaminePatchApplicationServiceTests
{
    [Test]
    public async Task PreviewReplayUsesTheSameEntryMainUpdateAsApprovedWrites()
    {
        var applier = CreateApplierWithRealEntryService();
        var entry = new Entry();
        var context = JsonConvert.SerializeObject(new ExamineMain
        {
            Items =
            [
                new() { Key = nameof(Entry.Name), Value = "patched-name" },
                new() { Key = nameof(Entry.DisplayName), Value = "Patched display" },
                new() { Key = nameof(Entry.BriefIntroduction), Value = "Patched brief" }
            ]
        });

        await applier.ApplyAsync(entry, new Examine
        {
            Operation = Operation.EstablishMain,
            Context = context
        });

        var projector = new RecordPreviewProjector(typeof(Entry));
        projector.Load(entry);
        var snapshot = projector.Snapshot(value => value);

        Assert.Multiple(() =>
        {
            Assert.That(entry.Name, Is.EqualTo("patched-name"));
            Assert.That(snapshot.Items.Any(item => item.Key == "main/Name" && item.DisplayValue == "patched-name"), Is.True);
            Assert.That(snapshot.Items.Any(item => item.Key == "main/DisplayName" && item.DisplayValue == "Patched display"), Is.True);
        });
    }

    [Test]
    public async Task MainPageAndImagesUseTheSharedEntryApplicationMethods()
    {
        var applier = CreateApplierWithRealEntryService();
        var entry = new Entry();

        await applier.ApplyAsync(entry, new Examine
        {
            Operation = Operation.EstablishMainPage,
            Context = "## Body"
        });
        await applier.ApplyAsync(entry, new Examine
        {
            Operation = Operation.EstablishImages,
            Context = JsonConvert.SerializeObject(new CnGalWebSite.DataModel.ExamineModel.Entries.EntryImages
            {
                Images =
                [
                    new() { Url = "https://example.com/a.png", Note = "封面", Priority = 10 }
                ]
            })
        });

        var projector = new RecordPreviewProjector(typeof(Entry));
        projector.Load(entry);
        var snapshot = projector.Snapshot(value => value);

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.Items.Any(item => item.Kind == RecordPreviewKind.Body && item.Value == "## Body"), Is.True);
            Assert.That(snapshot.Items.Any(item => item.Kind == RecordPreviewKind.Image && item.Value == "https://example.com/a.png"), Is.True);
        });
    }

    private static ExaminePatchApplicationService CreateApplierWithRealEntryService()
    {
        // The main/page/image entry updaters are pure in-memory methods. An uninitialized
        // service instance lets the test exercise the same method without constructing the
        // web host's repository graph.
        var entryService = (IEntryService)RuntimeHelpers.GetUninitializedObject(typeof(EntryService));
        return new ExaminePatchApplicationService(
            entryService, null!, null!, null!, null!, null!, null!, null!, null!);
    }
}
