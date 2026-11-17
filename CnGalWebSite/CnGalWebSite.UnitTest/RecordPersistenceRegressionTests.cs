using System.Collections.Concurrent;
using System.Data.Common;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Examines;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Stores;
using CnGalWebSite.APIServer.Application.Tags;
using CnGalWebSite.APIServer.Application.Videos;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Controllers;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.Infrastructure;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.SDK.MainSite.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CnGalWebSite.UnitTest;

/// <summary>Real domain services and repositories; only external presentation/network dependencies are replaced.</summary>
public class RecordPersistenceRegressionTests
{
    private SqliteConnection keeper = null!;
    private DbContextOptions<AppDbContext> options = null!;
    private int entryId;
    private int tagId;
    private const string Promo = "![promo](https://example.com/promo.png)\n\nbody";

    private static readonly Operation[] Parts = [Operation.EstablishMain, Operation.EstablishMainPage,
        Operation.EstablishAddInfor, Operation.EstablishImages, Operation.EstablishAudio,
        Operation.EstablishWebsite, Operation.EstablishTags, Operation.EstablishRelevances];

    [SetUp]
    public async Task SetUp()
    {
        var connectionString = $"Data Source=record_{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=False";
        keeper = new SqliteConnection(connectionString);
        await keeper.OpenAsync();
        options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connectionString).Options;
        await using var db = Context();
        await db.Database.EnsureCreatedAsync();
        var entry = new Entry { Name = "original", MainPicture = "A.png", MainPage = "body", ReaderCount = 17, Priority = 9 };
        var tag = new Tag { Name = "section-tag" };
        db.AddRange(entry, tag);
        db.EntryInformationTypes.Add(new EntryInformationType { Name = "QQ群", Icon = "qq" });
        await db.SaveChangesAsync();
        entryId = entry.Id;
        tagId = tag.Id;
    }

    [TearDown]
    public void TearDown() => keeper.Dispose();

    private AppDbContext Context(SqlCapture? capture = null) => capture is null ? new(options)
        : new(new DbContextOptionsBuilder<AppDbContext>(options).AddInterceptors(capture).Options);

    public static IEnumerable<TestCaseData> PartPairs()
    {
        foreach (var first in Parts)
        foreach (var second in Parts.Where(p => p != first))
            yield return new TestCaseData(first, second);
    }

    [TestCaseSource(nameof(PartPairs))]
    public Task IndependentlyLoadedSectionsPreserveBothWrites(Operation first, Operation second) => SavePartsAsync([first, second]);

    [Test]
    public Task AllEightSectionsCanBeSubmittedTogether() => SavePartsAsync(Parts);

    private async Task SavePartsAsync(Operation[] parts)
    {
        var allLoaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var saved = parts.Select(_ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).ToArray();
        var loaded = 0;
        await Task.WhenAll(parts.Select(async (operation, index) =>
        {
            var capture = new SqlCapture();
            await using var db = Context(capture);
            var target = await LoadEntryAsync(db);
            if (Interlocked.Increment(ref loaded) == parts.Length) allLoaded.SetResult();
            await allLoaded.Task;
            if (index > 0) await saved[index - 1].Task;
            try
            {
                var services = new RealRecordServices(db);
                await services.Get<ExamineService>().ApplyEditRecordToObject(target, Data(operation), operation);
                var allowed = new HashSet<string> { "LastEditTime" };
                if (operation == Operation.EstablishMain) allowed.Add("Name");
                if (operation == Operation.EstablishMainPage) allowed.Add("MainPage");
                if (operation == Operation.EstablishAddInfor) allowed.Add("PubulishTime");
                foreach (var sql in capture.Commands.Where(sql => sql.StartsWith("UPDATE \"Entries\"", StringComparison.Ordinal)))
                {
                    var set = sql[..sql.IndexOf("WHERE", StringComparison.Ordinal)];
                    var columns = Regex.Matches(set, "\"(\\w+)\" =").Select(m => m.Groups[1].Value);
                    Assert.That(columns.All(allowed.Contains), Is.True, $"{operation} wrote unrelated columns: {sql}");
                }
            }
            finally { saved[index].SetResult(); }
        }));
        await using var verify = Context();
        var result = await LoadEntryAsync(verify);
        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(parts.Contains(Operation.EstablishMain) ? "renamed" : "original"));
            Assert.That(result.MainPage, Is.EqualTo(parts.Contains(Operation.EstablishMainPage) ? Promo : "body"));
            Assert.That(result.Releases.Count, Is.EqualTo(parts.Contains(Operation.EstablishAddInfor) ? 1 : 0));
            Assert.That(result.Pictures.Count, Is.EqualTo(parts.Contains(Operation.EstablishImages) ? 1 : 0));
            Assert.That(result.Audio.Count, Is.EqualTo(parts.Contains(Operation.EstablishAudio) ? 1 : 0));
            Assert.That(result.WebsiteAddInfor?.Introduction, Is.EqualTo(parts.Contains(Operation.EstablishWebsite) ? "intro" : null));
            Assert.That(result.Tags.Count, Is.EqualTo(parts.Contains(Operation.EstablishTags) ? 1 : 0));
            Assert.That(result.Outlinks.Count, Is.EqualTo(parts.Contains(Operation.EstablishRelevances) ? 1 : 0));
            Assert.That(result.MainPicture, Is.EqualTo("A.png"));
            Assert.That(result.ReaderCount, Is.EqualTo(17));
            Assert.That(result.Priority, Is.EqualTo(9));
        });
    }

    private object Data(Operation operation) => operation switch
    {
        Operation.EstablishMain => new ExamineMain { Items = [new() { Key = "Name", Value = "renamed" }] },
        Operation.EstablishMainPage => Promo,
        Operation.EstablishAddInfor => new EntryAddInfor { Releases = [new() { Name = "release", PublishPlatformType = PublishPlatformType.Other, Link = "https://example.com/", Time = DateTime.UtcNow.Date }] },
        Operation.EstablishImages => new EntryImages { Images = [new() { Url = "image.png" }] },
        Operation.EstablishAudio => new EntryAudioExamineModel { Audio = [new() { Url = "audio.ogg", Name = "audio" }] },
        Operation.EstablishWebsite => new EntryWebsiteExamineModel { MainInfor = [new() { Key = "Introduction", Value = "intro" }] },
        Operation.EstablishTags => new EntryTags { Tags = [new() { TagId = tagId }] },
        _ => new EntryRelevances { Relevances = [new() { Type = RelevancesType.Outlink, DisplayName = "link", Link = "https://example.com/" }] }
    };

    private Task<Entry> LoadEntryAsync(AppDbContext db) => db.Entries.Include(e => e.Information).Include(e => e.Releases)
        .Include(e => e.Pictures).Include(e => e.Audio).Include(e => e.WebsiteAddInfor).ThenInclude(w => w.Images)
        .Include(e => e.Booking).ThenInclude(b => b.Goals).Include(e => e.Tags).Include(e => e.Outlinks)
        .Include(e => e.EntryStaffFromEntryNavigation).Include(e => e.EntryRelationFromEntryNavigation)
        .Include(e => e.Articles).Include(e => e.Videos).AsSplitQuery().SingleAsync(e => e.Id == entryId);

    [TestCase(Operation.EstablishMain, typeof(Entry))]
    [TestCase(Operation.EditArticleMain, typeof(Article))]
    [TestCase(Operation.EditVideoMain, typeof(Video))]
    [TestCase(Operation.EditPeripheryMain, typeof(Periphery))]
    [TestCase(Operation.EditTagMain, typeof(Tag))]
    public async Task ExplicitMainFieldWritesEvenWhenEqualToLoadedValue(Operation operation, Type type)
    {
        object id;
        await using (var seed = Context())
        {
            var model = Activator.CreateInstance(type)!;
            type.GetProperty("Name")!.SetValue(model, "main-overwrite");
            type.GetProperty("MainPicture")!.SetValue(model, "A.png");
            seed.Add(model);
            await seed.SaveChangesAsync();
            id = type.GetProperty("Id")!.GetValue(model)!;
        }
        await using var first = Context();
        await using var second = Context();
        var firstValue = (await first.FindAsync(type, id))!;
        var secondValue = (await second.FindAsync(type, id))!;
        type.GetProperty("MainPicture")!.SetValue(firstValue, "B.png");
        await first.SaveChangesAsync();
        var data = new ExamineMain { Items = [new() { Key = "MainPicture", Value = "A.png" }] };
        await new RealRecordServices(second).Get<ExamineService>().ApplyEditRecordToObject(secondValue, data, operation);
        await using var verify = Context();
        var result = (await verify.FindAsync(type, id))!;
        Assert.That(type.GetProperty("MainPicture")!.GetValue(result), Is.EqualTo("A.png"));
    }

    [TestCase("BookingCount")]
    [TestCase("EntryId")]
    [TestCase("Id")]
    public void NestedBookingCommandsCannotWriteCountersOrOwnership(string key)
    {
        using var db = Context();
        var entry = db.Entries.Single(e => e.Id == entryId);
        var data = new EntryAddInfor();
        data.Booking.MainInfor.Add(new() { Key = key, Value = "999" });
        Assert.That(() => new EditWriteBoundary(db, entry, Operation.EstablishAddInfor, data), Throws.InvalidOperationException);
    }

    [TestCase("EntryId")]
    [TestCase("Id")]
    public void NestedWebsiteCommandsCannotWriteOwnership(string key)
    {
        using var db = Context();
        var entry = db.Entries.Single(e => e.Id == entryId);
        var data = new EntryWebsiteExamineModel { MainInfor = [new() { Key = key, Value = "999" }] };
        Assert.That(() => new EditWriteBoundary(db, entry, Operation.EstablishWebsite, data), Throws.InvalidOperationException);
    }

    [Test]
    public async Task OwnedBookingCounterIsRejectedAtSaveEvenWithoutACommandKey()
    {
        await using var db = Context();
        var entry = await LoadEntryAsync(db);
        entry.Booking = new Booking { BookingCount = 10 };
        await db.SaveChangesAsync();
        using var boundary = new EditWriteBoundary(db, entry, Operation.EstablishAddInfor, new EntryAddInfor());
        entry.Booking.BookingCount = 999;
        Assert.That(async () => await db.SaveChangesAsync(), Throws.InvalidOperationException);
    }

    [Test]
    public async Task HistoryAndOverviewShareSnapshotsAndPreservePendingChanges()
    {
        await using (var seed = Context())
        {
            var time = DateTime.UtcNow.AddHours(-1);
            seed.Users.Add(new ApplicationUser { Id = "history-editor", UserName = "history-editor" });
            seed.Examines.AddRange(
                new Examine { Id = 162736, EntryId = entryId, Operation = Operation.EstablishMainPage, Context = "body", IsPassed = true, PassedTime = time, ApplyTime = time },
                new Examine { Id = 162737, EntryId = entryId, Operation = Operation.EstablishMainPage, Context = Promo, IsPassed = true, PassedTime = time.AddMinutes(1), ApplyTime = time },
                new Examine { Id = 162738, EntryId = entryId, Operation = Operation.EstablishAddInfor, Context = JsonConvert.SerializeObject(Data(Operation.EstablishAddInfor)), IsPassed = true, PassedTime = time.AddMinutes(2), ApplyTime = time },
                new Examine { Id = 162739, EntryId = entryId, Operation = Operation.EstablishMainPage, Context = Promo.Replace("\n\n", "\n"), IsPassed = true, PassedTime = time.AddMinutes(3), ApplyTime = time });
            foreach (var tracked in seed.ChangeTracker.Entries<Examine>()) tracked.Entity.ApplicationUserId = "history-editor";
            await seed.SaveChangesAsync();
        }
        await using var db = Context();
        var pending = await db.Tags.SingleAsync(t => t.Id == tagId);
        pending.Name = "pending-change";
        var services = new RealRecordServices(db);
        var history = services.Get<RecordHistoryService>();
        var examine = services.Get<ExamineService>();
        var records = await db.Examines.AsNoTracking().OrderBy(e => e.Id).ToListAsync();
        // Exercise the actual overview endpoint, not an imitation of its loop.
        var overview = (await services.Get<ExaminesAPIController>().GetExaminesOverview(162737)).Value!;
        foreach (var record in records)
        {
            var detail = new ExamineViewModel();
            await examine.GetExamineView(detail, record);
            Assert.That(overview.Examines.Single(r => r.Id == record.Id).EditOverview, Is.EqualTo(detail.EditOverview));
        }
        var added = (await history.GetFrameAsync(records.Single(r => r.Id == 162737)))!;
        Assert.That(added.BeforeMainPage, Does.Not.Contain("promo"));
        Assert.That(added.AfterMainPage, Does.Contain("promo"));
        var addInfo = (await history.GetFrameAsync(records.Single(r => r.Id == 162738)))!;
        Assert.That(addInfo.BeforeMainPage, Is.EqualTo(addInfo.AfterMainPage));
        var newline = (await history.GetFrameAsync(records.Single(r => r.Id == 162739)))!;
        Assert.That(newline.BeforeMainPage!.Replace("\n\n", "\n"), Is.EqualTo(newline.AfterMainPage));
        Assert.That(db.Entry(pending).State, Is.EqualTo(EntityState.Modified));
        Assert.That(db.ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).Select(e => e.Entity), Is.EqualTo(new[] { pending }));
        await db.SaveChangesAsync();
        await using var verify = Context();
        Assert.That((await verify.Tags.SingleAsync(t => t.Id == tagId)).Name, Is.EqualTo("pending-change"));
    }

    [Test]
    public async Task StoreRefreshPersistsDetachedMetricsWithoutOverwritingIdentity()
    {
        await using var db = Context();
        var store = new StoreInfo { EntryId = entryId, Name = "release", Link = "original", PlatformType = PublishPlatformType.Other };
        db.StoreInfo.Add(store);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var stale = await db.StoreInfo.AsNoTracking().SingleAsync();
        stale.Name = "must-not-write";
        stale.Link = "must-not-write";
        stale.PriceNow = 42;
        var service = new RealRecordServices(db).Get<StoreInfoService>();
        await service.SaveRefreshedAsync(stale);
        await using var verify = Context();
        var result = await verify.StoreInfo.SingleAsync();
        Assert.That(result.PriceNow, Is.EqualTo(42));
        Assert.That(result.Name, Is.EqualTo("release"));
        Assert.That(result.Link, Is.EqualTo("original"));
    }

    [Test]
    public async Task MissingStoreLookupCannotDetachPendingWrites()
    {
        await using var db = Context();
        var pending = await db.Tags.SingleAsync(t => t.Id == tagId);
        pending.Name = "preserved-through-store-read";
        using (new RecordReadScope(db))
            await new RealRecordServices(db).Get<StoreInfoService>().Get(PublishPlatformType.Other, null!, "link", "name", 0);
        Assert.That(db.Entry(pending).State, Is.EqualTo(EntityState.Modified));
        await db.SaveChangesAsync();
        await using var verify = Context();
        Assert.That((await verify.Tags.SingleAsync(t => t.Id == tagId)).Name, Is.EqualTo("preserved-through-store-read"));
    }

    [Test]
    public async Task DetachedIntegralRefreshUpdatesOnlyDerivedUserFields()
    {
        await using var db = Context();
        db.Users.Add(new ApplicationUser { Id = "integral-user", UserName = "original-name", GCoins = 7, DisplayIntegral = 9 });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var user = await db.Users.AsNoTracking().SingleAsync(u => u.Id == "integral-user");
        user.UserName = "must-not-write";
        await new RealRecordServices(db).Get<UserService>().UpdateUserIntegral(user);
        await using var verify = Context();
        var result = await verify.Users.SingleAsync(u => u.Id == user.Id);
        Assert.That(result.UserName, Is.EqualTo("original-name"));
        Assert.That(result.GCoins, Is.Zero);
        Assert.That(result.DisplayIntegral, Is.Zero);
    }

    [Test]
    public async Task RejectedDetachedFieldSelectionDoesNotAttachTheEntity()
    {
        await using var db = Context();
        var detached = await db.Tags.AsNoTracking().SingleAsync(t => t.Id == tagId);
        var repository = new RepositoryBase<Tag, int>(db);
        Assert.That(() => repository.UpdateFields(detached, t => t.Id), Throws.ArgumentException);
        Assert.That(db.Entry(detached).State, Is.EqualTo(EntityState.Detached));
    }

    [Test]
    public async Task ReadScopeRejectsAttemptsToClearPendingWrites()
    {
        await using var db = Context();
        var tag = await db.Tags.SingleAsync(t => t.Id == tagId);
        using var read = new RecordReadScope(db);
        Assert.That(() => new RepositoryBase<Tag, int>(db).Clear(), Throws.InvalidOperationException);
        Assert.That(db.Entry(tag).State, Is.EqualTo(EntityState.Unchanged));
    }

    [Test]
    public async Task OwnershipCannotBeTransferredFromAnotherEntry()
    {
        await using var db = Context();
        var target = await LoadEntryAsync(db);
        var other = new Entry { Name = "other-owner", Pictures = [new EntryPicture { Url = "other.png" }] };
        db.Entries.Add(other);
        await db.SaveChangesAsync();
        using var boundary = new EditWriteBoundary(db, target, Operation.EstablishImages, new EntryImages());
        target.Pictures.Add(other.Pictures.Single());
        Assert.That(async () => await db.SaveChangesAsync(), Throws.InvalidOperationException);
    }

    private static readonly Operation[] PendingOperations = [Operation.EstablishMainPage, Operation.EditArticleMainPage,
        Operation.EditVideoMainPage, Operation.EditTagMain, Operation.DisambigMain, Operation.UserMainPage,
        Operation.EditPeripheryMain, Operation.EditPlayedGameMain, Operation.RequestUserCertification, Operation.EditFavoriteFolderMain];

    private async Task<(object Target, ApplicationUser Author)> PendingTargetAsync(AppDbContext db, Operation operation)
    {
        var author = new ApplicationUser { Id = "editor", UserName = "editor" };
        db.Users.Add(author);
        object target = operation switch
        {
            Operation.EditArticleMainPage => new Article { Name = "article" },
            Operation.EditVideoMainPage => new Video { Name = "video" },
            Operation.EditTagMain => await db.Tags.SingleAsync(t => t.Id == tagId),
            Operation.DisambigMain => new Disambig { Name = "disambig" },
            Operation.UserMainPage => author,
            Operation.EditPeripheryMain => new Periphery { Name = "periphery" },
            Operation.EditPlayedGameMain => new PlayedGame { EntryId = entryId, ApplicationUser = author },
            Operation.RequestUserCertification => new UserCertification(),
            Operation.EditFavoriteFolderMain => new FavoriteFolder { Name = "folder", ApplicationUser = author },
            _ => await LoadEntryAsync(db)
        };
        if (target is not UserCertification && db.Entry(target).State == EntityState.Detached) db.Add(target);
        await db.SaveChangesAsync();
        return (target, author);
    }

    [TestCaseSource(nameof(PendingOperations))]
    public async Task UpdatingPendingSubmissionWorksWithStrictDetachedUpdates(Operation operation)
    {
        await using var db = Context();
        var (target, author) = await PendingTargetAsync(db, operation);
        var service = new RealRecordServices(db).Get<ExamineService>();
        var initial = await service.AddEditRecordAsync(target, author, "first", operation, "note", false);
        initial.ApplyTime = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();
        var originalTime = initial.ApplyTime;
        var updated = await service.AddEditRecordAsync(target, author, "second", operation, "note", false);
        Assert.That(updated.Id, Is.EqualTo(initial.Id));
        Assert.That(updated.Context, Is.EqualTo(operation is Operation.EstablishMainPage or Operation.EditArticleMainPage
            or Operation.EditVideoMainPage or Operation.UserMainPage ? "second" : JsonConvert.SerializeObject("second")));
        Assert.That(updated.ApplyTime, Is.GreaterThan(originalTime));
        Assert.That(db.ChangeTracker.HasChanges(), Is.False);
        await db.SaveChangesAsync();
        await using var verify = Context();
        var stored = await verify.Examines.SingleAsync();
        Assert.That(stored.Context, Is.EqualTo(updated.Context));
        Assert.That(stored.ApplyTime, Is.EqualTo(updated.ApplyTime));
        Assert.That(stored.IsPassed, Is.Null);
    }

    private static IEnumerable<TestCaseData> ReviewedPendingOperations() =>
        from operation in PendingOperations
        from passed in new[] { true, false }
        select new TestCaseData(operation, passed);

    [TestCaseSource(nameof(ReviewedPendingOperations))]
    public async Task PendingSubmissionCannotOverwriteARecordReviewedAfterLookup(Operation operation, bool passed)
    {
        var capture = new SqlCapture();
        await using var db = Context(capture);
        var (target, author) = await PendingTargetAsync(db, operation);
        var service = new RealRecordServices(db).Get<ExamineService>();
        var initial = await service.AddEditRecordAsync(target, author, "first", operation, "note", false);
        var originalContext = initial.Context;
        var originalTime = initial.ApplyTime;
        var claimed = false;
        // Commit the real approval claim after the submitter's pending lookup and before its UPDATE.
        capture.BeforeExamineUpdate = async () =>
        {
            await using var reviewer = Context();
            var record = await reviewer.Examines.SingleAsync();
            var unitOfWork = new RealRecordServices(reviewer).Get<EditRecordUnitOfWork>();
            claimed = await unitOfWork.RunAsync(() => unitOfWork.TryClaimAsync(record, passed));
        };

        Assert.That(async () => await service.AddEditRecordAsync(target, author, "second", operation, "note", false),
            Throws.InvalidOperationException.With.Message.EqualTo("该记录已经被审核，不能修改审核状态"));
        Assert.That(claimed, Is.True);
        // A later save in this context must not leak the rejected submission into history either.
        await db.SaveChangesAsync();
        await using var verify = Context();
        var stored = await verify.Examines.SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(stored.Context, Is.EqualTo(originalContext));
            Assert.That(stored.ApplyTime, Is.EqualTo(originalTime));
            Assert.That(stored.IsPassed, Is.EqualTo(passed));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task EntryHistoryCapturesUniqueNameChangesWithAnUnchangedDisplayName(bool prepareFrames)
    {
        await using var db = Context();
        var time = DateTime.UtcNow.AddHours(-1);
        var first = new Examine
        {
            EntryId = entryId, Operation = Operation.EstablishMain, IsPassed = true,
            ApplyTime = time, PassedTime = time,
            Context = JsonConvert.SerializeObject(new ExamineMain
            {
                Items = [new() { Key = "Name", Value = "old-unique" }, new() { Key = "DisplayName", Value = "display" }]
            })
        };
        var second = new Examine
        {
            EntryId = entryId, Operation = Operation.EstablishMain, IsPassed = true,
            ApplyTime = time.AddMinutes(1), PassedTime = time.AddMinutes(1),
            Context = JsonConvert.SerializeObject(new ExamineMain { Items = [new() { Key = "Name", Value = "new-unique" }] })
        };
        db.Examines.AddRange(first, second);
        await db.SaveChangesAsync();
        var history = new RealRecordServices(db).Get<RecordHistoryService>();
        var comparison = await history.CompareAsync<EntryIndexViewModel>(second, first.Id);
        Assert.That(comparison.Success, Is.True);
        Assert.That(comparison.Before.Name, Is.EqualTo("display"));
        Assert.That(comparison.After.Name, Is.EqualTo("display"));
        if (prepareFrames) await history.PrepareFramesAsync([second]);
        var frame = (await history.GetFrameAsync(second))!;
        var diff = RecordDiffBuilder.Build(ExaminePreviewMapper.FromPreData(frame.Before), ExaminePreviewMapper.FromPreData(frame.After));
        var nameChange = diff.Items.Single(i => i.Item.Label == "唯一名称");
        Assert.Multiple(() =>
        {
            Assert.That(nameChange.Change, Is.EqualTo(RecordChangeKind.Modified));
            Assert.That(nameChange.Before!.DisplayValue, Is.EqualTo("old-unique"));
            Assert.That(nameChange.After!.DisplayValue, Is.EqualTo("new-unique"));
            Assert.That(diff.Items.Single(i => i.Item.Label == "名称").Change, Is.EqualTo(RecordChangeKind.Unchanged));
        });
    }

    private sealed class SqlCapture : DbCommandInterceptor
    {
        public ConcurrentQueue<string> Commands { get; } = new();
        public Func<Task>? BeforeExamineUpdate { get; set; }
        private async Task CaptureAsync(DbCommand command)
        {
            Commands.Enqueue(command.CommandText);
            if (BeforeExamineUpdate is { } action && command.CommandText.StartsWith("UPDATE \"Examines\"", StringComparison.Ordinal))
            {
                BeforeExamineUpdate = null;
                await action();
            }
        }
        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken token = default)
        { await CaptureAsync(command); return result; }
        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken token = default)
        { await CaptureAsync(command); return result; }
    }
}

internal sealed class RealRecordServices(AppDbContext db)
{
    private readonly Dictionary<Type, object> instances = new();
    public T Get<T>() => (T)Create(typeof(T));
    private object Create(Type type)
    {
        if (instances.TryGetValue(type, out var value)) return value;
        var instance = Activator.CreateInstance(type, type.GetConstructors()[0].GetParameters().Select(p => Resolve(p.ParameterType)).ToArray())!;
        instances.Add(type, instance);
        return instance;
    }
    private object? Resolve(Type type)
    {
        if (type == typeof(AppDbContext)) return db;
        if (type == typeof(IAppHelper)) return DispatchProxy.Create<IAppHelper, RecordPresentationProxy>();
        if (type == typeof(IStoreInfoService)) return DispatchProxy.Create<IStoreInfoService, NoStoreLookupProxy>();
        if (type == typeof(IEntryService)) return Get<EntryService>();
        if (type == typeof(IArticleService)) return Get<ArticleService>();
        if (type == typeof(ITagService)) return Get<TagService>();
        if (type == typeof(IPeripheryService)) return Get<PeripheryService>();
        if (type == typeof(IVideoService)) return Get<VideoService>();
        if (type == typeof(IExamineService)) return Get<ExamineService>();
        if (type == typeof(RecordHistoryService)) return Get<RecordHistoryService>();
        if (type == typeof(EditRecordUnitOfWork)) return Get<EditRecordUnitOfWork>();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRepository<,>))
            return Activator.CreateInstance(typeof(RepositoryBase<,>).MakeGenericType(type.GetGenericArguments()), db);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ILogger<>))
            return typeof(NullLogger<>).MakeGenericType(type.GetGenericArguments()).GetField("Instance")!.GetValue(null);
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IOptions<>))
            return Activator.CreateInstance(typeof(OptionsWrapper<>).MakeGenericType(type.GetGenericArguments()), Activator.CreateInstance(type.GenericTypeArguments[0]));
        return null;
    }
}

public class RecordPresentationProxy : DispatchProxy
{
    protected override object? Invoke(MethodInfo? method, object?[]? args) => method!.Name switch
    {
        "GetImagePath" => args![0] as string ?? "",
        "MarkdownToHtml" => Markdig.Markdown.ToHtml(args![0] as string ?? ""),
        "GetJsonStringView" => "<pre><code class=\"language-json\">" + WebUtility.HtmlEncode(args![0] as string ?? "") + "</code></pre>",
        "GetAPICurrentUserAsync" => Task.FromResult<ApplicationUser?>(null),
        _ => method.ReturnType.IsValueType ? Activator.CreateInstance(method.ReturnType) : null
    };
}

public class NoStoreLookupProxy : DispatchProxy
{
    protected override object? Invoke(MethodInfo? method, object?[]? args)
        => throw new InvalidOperationException("Historical projection must not query live store data: " + method!.Name);
}
